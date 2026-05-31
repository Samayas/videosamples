import gc
import os
import signal
import sys
import threading
import time
import numpy as np
import pyaudiowpatch as pyaudio
from collections import deque
from typing import Deque, List, Optional, Tuple
from faster_whisper import WhisperModel

DEFAULT_MODEL_SIZE = "base"
TARGET_SAMPLE_RATE = 16000
CHUNK_FRAMES = 1024

TRANSCRIBE_WINDOW_SECONDS = 4.0
STEP_SECONDS = 1.0
OVERLAP_SECONDS = 1.0
MAX_BUFFER_SECONDS = 20.0

stop_requested = False

def handle_stop_signal(signum, frame) -> None:
    global stop_requested
    stop_requested = True

def resample_audio(audio: np.ndarray, source_rate: int, target_rate: int) -> np.ndarray:
    if source_rate == target_rate:
        return audio.astype(np.float32, copy=False)

    if len(audio) == 0:
        return np.array([], dtype=np.float32)

    duration_seconds = len(audio) / float(source_rate)
    target_length = max(1, int(round(duration_seconds * target_rate)))

    source_positions = np.linspace(0.0, 1.0, num=len(audio), endpoint=False)
    target_positions = np.linspace(0.0, 1.0, num=target_length, endpoint=False)

    resampled_audio = np.interp(target_positions, source_positions, audio)
    return resampled_audio.astype(np.float32, copy=False)

def pcm16_bytes_to_mono_float32(raw_data: bytes, channels: int) -> np.ndarray:
    pcm_audio = np.frombuffer(raw_data, dtype=np.int16)

    if channels > 1:
        pcm_audio = pcm_audio.reshape(-1, channels)
        mono_audio = pcm_audio.mean(axis=1)
    else:
        mono_audio = pcm_audio.astype(np.float32)

    normalized_audio = mono_audio.astype(np.float32) / 32768.0
    normalized_audio = np.clip(normalized_audio, -1.0, 1.0)
    return normalized_audio

def normalize_text(value: str) -> str:
    return " ".join(value.strip().lower().split())

class RollingAudioBuffer:
    def __init__(self, max_seconds: float) -> None:
        self.max_seconds = max_seconds
        self.chunks: Deque[np.ndarray] = deque()
        self.total_samples = 0
        self.lock = threading.Lock()

    def append(self, audio_chunk: np.ndarray) -> None:
        with self.lock:
            self.chunks.append(audio_chunk)
            self.total_samples += len(audio_chunk)

            max_samples = int(self.max_seconds * TARGET_SAMPLE_RATE)
            while self.total_samples > max_samples and len(self.chunks) > 0:
                removed = self.chunks.popleft()
                self.total_samples -= len(removed)

    def get_last_seconds(self, seconds: float) -> np.ndarray:
        requested_samples = int(seconds * TARGET_SAMPLE_RATE)

        with self.lock:
            if self.total_samples == 0:
                return np.array([], dtype=np.float32)

            collected: List[np.ndarray] = []
            accumulated = 0

            for chunk in reversed(self.chunks):
                collected.append(chunk)
                accumulated += len(chunk)
                if accumulated >= requested_samples:
                    break

        merged = np.concatenate(list(reversed(collected)))
        if len(merged) > requested_samples:
            merged = merged[-requested_samples:]

        return merged.astype(np.float32, copy=False)

def audio_capture_worker(audio_buffer: RollingAudioBuffer) -> Tuple[threading.Thread, pyaudio.PyAudio]:
    audio_interface = pyaudio.PyAudio()

    try:
        speaker_info = audio_interface.get_default_wasapi_loopback()
    except Exception:
        audio_interface.terminate()
        raise

    input_device_index = int(speaker_info["index"])
    input_channels = int(speaker_info["maxInputChannels"])
    input_sample_rate = int(speaker_info["defaultSampleRate"])
    sample_format = pyaudio.paInt16

    print(f"Recording continuously from: ({input_device_index}) {speaker_info['name']}")
    print(f"Input format: {input_channels} channel(s), {input_sample_rate} Hz, 16-bit PCM")
    print("Press Ctrl+C to stop.\n")

    stream = audio_interface.open(
        format=sample_format,
        channels=input_channels,
        rate=input_sample_rate,
        input=True,
        input_device_index=input_device_index,
        frames_per_buffer=CHUNK_FRAMES,
    )

    def worker() -> None:
        global stop_requested

        try:
            while not stop_requested:
                raw_data = stream.read(CHUNK_FRAMES, exception_on_overflow=False)
                mono_audio = pcm16_bytes_to_mono_float32(raw_data, input_channels)
                resampled_audio = resample_audio(mono_audio, input_sample_rate, TARGET_SAMPLE_RATE)
                audio_buffer.append(resampled_audio)
        finally:
            try:
                stream.stop_stream()
            except Exception:
                pass
            try:
                stream.close()
            except Exception:
                pass
            audio_interface.terminate()

    thread = threading.Thread(target=worker, daemon=True)
    thread.start()
    return thread, audio_interface

def transcribe_continuous(
    model_size: str = DEFAULT_MODEL_SIZE,
    models_dir: Optional[str] = None,
) -> None:
    model_path = os.path.join(models_dir, model_size) if models_dir else model_size

    print(f"Loading model: {model_path}")
    model = WhisperModel(model_path, device="cpu", compute_type="int8")

    audio_buffer = RollingAudioBuffer(max_seconds=MAX_BUFFER_SECONDS)
    capture_thread, _ = audio_capture_worker(audio_buffer)

    last_transcription_time = 0.0
    recent_lines: Deque[str] = deque(maxlen=20)
    total_audio_offset = 0.0

    try:
        while not stop_requested:
            now = time.time()
            if now - last_transcription_time < STEP_SECONDS:
                time.sleep(0.05)
                continue

            last_transcription_time = now
            window_audio = audio_buffer.get_last_seconds(TRANSCRIBE_WINDOW_SECONDS)

            minimum_samples = int(1.0 * TARGET_SAMPLE_RATE)
            if len(window_audio) < minimum_samples:
                continue

            try:
                segments, info = model.transcribe(
                    window_audio,
                    vad_filter=True,
                    vad_parameters={
                        "min_silence_duration_ms": 500,
                        "speech_pad_ms": 150,
                        "min_speech_duration_ms": 150,
                    },
                    beam_size=1,
                    condition_on_previous_text=False,
                )
            except ValueError:
                continue

            emitted_any = False
            for segment in segments:
                clean_text = segment.text.strip()
                if not clean_text:
                    continue

                normalized = normalize_text(clean_text)
                if normalized in recent_lines:
                    continue

                recent_lines.append(normalized)
                emitted_any = True

                print(f"{clean_text}")

            total_audio_offset += STEP_SECONDS

            if not emitted_any:
                pass
    finally:
        try:
            capture_thread.join(timeout=2.0)
        except Exception:
            pass

        del model
        gc.collect()

if __name__ == "__main__":
    signal.signal(signal.SIGINT, handle_stop_signal)
    if hasattr(signal, "SIGTERM"):
        signal.signal(signal.SIGTERM, handle_stop_signal)

    try:
        transcribe_continuous()
    except KeyboardInterrupt:
        stop_requested = True
        print("\nStopping...")
        sys.exit(0)