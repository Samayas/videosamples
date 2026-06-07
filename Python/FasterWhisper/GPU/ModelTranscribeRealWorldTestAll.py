import gc
import os
import time
from pathlib import Path
from faster_whisper import WhisperModel

INPUT_FILE = "D:\\Projects\\Samayas\\Videos\\1 Introduction\\Video\\1 Introduction - Youtube.mp4"
MODEL_SIZES = [
    "tiny",
    "tiny.en",
    "base",
    "base.en",
    "small",
    "small.en",
    "medium",
    "medium.en",
    "large-v1",
    "large-v2",
    "large-v3",
    "distil-large-v2",
    "distil-large-v3",
]

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

def transcribe(audio_file: str, model_size: str, models_dir: str = None) -> None:
    model_path = os.path.join(models_dir, model_size) if models_dir else model_size

    print(f"Loading model: {model_path}")
    model = WhisperModel(model_path, device="cuda", compute_type="float16")

    print(f"Transcribing: {audio_file}")

    segments, info = model.transcribe(audio_file,)

    print(f"Audio duration: {format_timestamp(info.duration)}")
    print(f"Detected language: {info.language} (probability: {info.language_probability:.2f})\n")

    transcription_start = time.time()

    for segment in segments:
        clean_text = segment.text.strip()

        start_time_str = format_timestamp(segment.start)
        end_time_str = format_timestamp(segment.end)

        timed_line = f"[{start_time_str} -> {end_time_str}] {clean_text}"

        print(timed_line)

    elapsed = time.time() - transcription_start
    print(f"\nTranscription completed in: {format_timestamp(elapsed)}")

    del model
    gc.collect()

    return elapsed

def run_all_models(audio_file: str, models_dir: str = None) -> None:
    total_models = len(MODEL_SIZES)
    overall_start = time.time()

    results: list[tuple[str, str]] = []

    for index, model_size in enumerate(MODEL_SIZES):
        separator = "=" * 60
        print(f"\n{separator}")
        print(f"  Model [{index + 1}/{total_models}]: {model_size}")
        print(f"{separator}\n")

        try:
            elapsed = transcribe(audio_file, model_size, models_dir)
            results.append((model_size, format_timestamp(elapsed)))
        except Exception as e:
            print(f"  Failed: {model_size} — {e}")
            results.append((model_size, "FAILED"))

    overall_elapsed = time.time() - overall_start

    col_model = max(len(row[0]) for row in results)
    col_model = max(col_model, len("Model"))
    col_time = max(len(row[1]) for row in results)
    col_time = max(col_time, len("Time taken"))

    row_separator = f"+{'-' * (col_model + 2)}+{'-' * (col_time + 2)}+"

    print(f"\n{row_separator}")
    print(f"| {'Model':<{col_model}} | {'Time taken':<{col_time}} |")
    print(f"|{'=' * (col_model + 2)}|{'=' * (col_time + 2)}|")
    for model_name, time_taken in results:
        print(f"| {model_name:<{col_model}} | {time_taken:<{col_time}} |")
    print(row_separator)

    print(f"\nAll models processed in: {format_timestamp(overall_elapsed)}")

if __name__ == "__main__":
    audio_path = Path(INPUT_FILE)
    if not audio_path.exists():
        print(f"Error: '{INPUT_FILE}' not found.")
    else:
        run_all_models(str(audio_path), models_dir=None)