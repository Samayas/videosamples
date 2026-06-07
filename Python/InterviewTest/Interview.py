import gc
import os
import signal
import sys
import threading
import time
import numpy as np
import pyaudiowpatch as pyaudio
import requests
import json
from collections import deque
from typing import Deque, List, Optional, Tuple
from faster_whisper import WhisperModel

# ... [Existing Constants] ...
DEFAULT_MODEL_SIZE = "base"
TARGET_SAMPLE_RATE = 16000
CHUNK_FRAMES = 1024
TRANSCRIBE_WINDOW_SECONDS = 3.0 # Reduced for faster detection
STEP_SECONDS = 0.5             # Faster polling
OVERLAP_SECONDS = 1.0
MAX_BUFFER_SECONDS = 20.0

# Interview Assistant Specific Constants
OLLAMA_URL = "http://192.168.150.149:11434/v1/chat/completions"
OLLAMA_MODEL = "google/gemma-4-12b"
OLLAMA_TOKEN = "sk-lm-bVdP5V81:FvgpGpCfJhFDefCr27ZA"
SILENCE_THRESHOLD_SECONDS = 1.5  # How long to wait for silence before assuming a question is asked

stop_requested = False

# ... [Existing resample_audio, pcm16_bytes_to_mono_float32, normalize_text functions] ...

def resample_audio(audio: np.ndarray, source_rate: int, target_rate: int) -> np.ndarray:
    if source_rate == target_rate: return audio.astype(np.float32, copy=False)
    if len(audio) == 0: return np.array([], dtype=np.float32)
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
    return np.clip(normalized_audio, -1.0, 1.0)

def normalize_text(value: str) -> str:
    return " ".join(value.strip().lower().split())

# ... [Existing RollingAudioBuffer class] ...

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
            if self.total_samples == 0: return np.array([], dtype=np.float32)
            collected: List[np.ndarray] = []
            accumulated = 0
            for chunk in reversed(self.chunks):
                collected.append(chunk)
                accumulated += len(chunk)
                if accumulated >= requested_samples: break
        merged = np.concatenate(list(reversed(collected)))
        if len(merged) > requested_samples: merged = merged[-requested_samples:]
        return merged.astype(np.float32, copy=False)

def query_llm(prompt: str) -> str:
    payload: dict = {
        "model": OLLAMA_MODEL,
        "messages": [
            {"role": "user", "content": f"You are an interview assistant. Provide a concise, highly technical answer (max 400 chars) to this question/statement: {prompt}"}
        ],
        "stream": False,
        "max_tokens": 4000
    }
    try:
        headers = {"Authorization": f"Bearer {OLLAMA_TOKEN}"}
        response = requests.post(OLLAMA_URL, json=payload, headers=headers, timeout=300)
        response.raise_for_status()
        return response.json().get("choices")[0].get("message").get("content").strip()
    except Exception as e:
        return f"Error: {str(e)}"

# ... [Existing audio_capture_worker] ...

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
    
    stream = audio_interface.open(
        format=pyaudio.paInt16,
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
            stream.stop_stream()
            stream.close()
            audio_interface.terminate()

    thread = threading.Thread(target=worker, daemon=True)
    thread.start()
    return thread, audio_interface

def transcribe_continuous(model_size: str = DEFAULT_MODEL_SIZE) -> None:
    print(f"Loading model...")
    model = WhisperModel(model_size, device="cpu", compute_type="int8")
    audio_buffer = RollingAudioBuffer(max_seconds=MAX_BUFFER_SECONDS)
    capture_thread, _ = audio_capture_worker(audio_buffer)

    last_transcription_time = 0.0
    recent_lines: Deque[str] = deque(maxlen=10)
    
    # --- Interview Logic Variables ---
    utterance_buffer: List[str] = []
    last_speech_detected_time = time.time()
    pipeline_start_time = None  # Tracks when first speech in current turn was detected

    try:
        while not stop_requested:
            now = time.time()
            if now - last_transcription_time < STEP_SECONDS:
                time.sleep(0.05)
                continue

            last_transcription_time = now
            t_capture = time.perf_counter()
            window_audio = audio_buffer.get_last_seconds(TRANSCRIBE_WINDOW_SECONDS)
            capture_ms = (time.perf_counter() - t_capture) * 1000

            if len(window_audio) < (int(1.0 * TARGET_SAMPLE_RATE)):
                continue

            try:
                t_transcribe = time.perf_counter()
                segments, _ = model.transcribe(window_audio, vad_filter=True, beam_size=1)
                transcribe_ms = (time.perf_counter() - t_transcribe) * 1000
            except Exception:
                continue

            has_speech_in_this_window = False
            for segment in segments:
                clean_text = segment.text.strip()
                if not clean_text: continue
                
                normalized = normalize_text(clean_text)
                if normalized in recent_lines: continue

                recent_lines.append(normalized)
                utterance_buffer.append(clean_text)
                has_speech_in_this_window = True
                if pipeline_start_time is None:
                    pipeline_start_time = time.perf_counter()
                print(f"[Live] (+{capture_ms:.0f}ms capture / +{transcribe_ms:.0f}s whisper): {clean_text}")

            # Logic: If we had speech, update the timestamp of last activity
            if has_speech_in_this_window:
                last_speech_detected_time = time.time()
            else:
                # Logic: If no speech was detected in this window AND 
                # we have something in our buffer, check if it's been silent long enough
                t_silence_check = time.time() - last_speech_detected_time
                if utterance_buffer and (t_silence_check > SILENCE_THRESHOLD_SECONDS):
                    full_query = " ".join(utterance_buffer)

                    # Clear buffer immediately to prevent double-triggering
                    utterance_buffer = []

                    t_llm_start = time.perf_counter()
                    answer = query_llm(full_query)
                    llm_ms = (time.perf_counter() - t_llm_start) * 1000

                    total_pipeline_ms = 0
                    if pipeline_start_time:
                        total_pipeline_ms = (time.perf_counter() - pipeline_start_time) * 1000
                        pipeline_start_time = None

                    print(f"\n>>> BUFFERED QUERY: {full_query}")
                    print(f">>> TIMING — silence wait: {t_silence_check:.2f}s | LLM call: {llm_ms:.0f}ms ({llm_ms/1000:.2f}s) | TOTAL pipeline: {total_pipeline_ms:.0f}ms ({total_pipeline_ms/1000:.2f}s)")
                    print(f">>> AI SUGGESTION: {answer}\n")

    finally:
        capture_thread.join(timeout=2.0)
        del model
        gc.collect()

if __name__ == "__main__":
    signal.signal(signal.SIGINT, lambda s, f: globals().update(stop_requested=True))
    try:
        transcribe_continuous()
    except KeyboardInterrupt:
        stop_requested = True
        sys.exit(0)
