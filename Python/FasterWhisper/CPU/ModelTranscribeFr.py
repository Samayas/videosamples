import gc
import os
from pathlib import Path
from faster_whisper import WhisperModel

DEFAULT_MODEL_SIZE = "base"
INPUT_FILE = "recordingFr.m4a"

def format_timestamp(seconds: float) -> str:
    hours = int(seconds // 3600)
    minutes = int((seconds % 3600) // 60)
    remaining_seconds = seconds % 60

    parts = []
    if hours > 0:
        parts.append(f"{hours}h")
    if minutes > 0:
        parts.append(f"{minutes}m")
    
    parts.append(f"{remaining_seconds:.2f}s")
    
    return " ".join(parts)

def transcribe(audio_file: str, models_dir: str = None) -> None:
    model_path = os.path.join(models_dir, DEFAULT_MODEL_SIZE) if models_dir else DEFAULT_MODEL_SIZE

    print(f"Loading model: {model_path}")
    model = WhisperModel(model_path, device="cpu", compute_type="int8")

    print(f"Transcribing: {audio_file}")
    segments, info = model.transcribe(audio_file)

    print(f"Audio duration: {format_timestamp(info.duration)}")
    print(f"Detected language: {info.language} (probability: {info.language_probability:.2f})\n")
    for segment in segments:
        clean_text = segment.text.strip()

        start_time_str = format_timestamp(segment.start)
        end_time_str = format_timestamp(segment.end)

        timed_line = f"[{start_time_str} -> {end_time_str}] {clean_text}"

        print(timed_line)

    del model
    gc.collect()

if __name__ == "__main__":
    audio_path = Path(INPUT_FILE)
    if not audio_path.exists():
        print(f"Error: '{INPUT_FILE}' not found.")
    else:
        transcribe(str(audio_path), models_dir=None)