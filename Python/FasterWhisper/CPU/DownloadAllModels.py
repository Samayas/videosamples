import gc
import os
import time
from pathlib import Path
from tqdm import tqdm
from huggingface_hub import snapshot_download
from faster_whisper import WhisperModel
from faster_whisper.utils import _MODELS

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

def short_path(full_path: str) -> str:
    try:
        relative = Path(full_path).relative_to(Path.home())
        parts = relative.parts
        snapshot_idx = next((i for i, p in enumerate(parts) if p == "snapshots"), None)
        trimmed = parts[:snapshot_idx] if snapshot_idx else parts
        return "~" + os.sep + os.path.join(*trimmed)
    except ValueError:
        return full_path
    
def download_all_faster_whisper_models(download_dir: str = None):
    total_models = len(MODEL_SIZES)
    print(f"Starting download of {total_models} models...")
    if download_dir:
        os.makedirs(download_dir, exist_ok=True)
        print(f"Saving to: {download_dir}")
    print("Note: This may take a while and requires significant disk space.\n")

    start_time = time.time()

    for index, size in enumerate(MODEL_SIZES):
        repo_id = _MODELS[size]
        tqdm.write(f"\n[{index + 1}/{total_models}] {size}  ({repo_id})")

        try:
            download_start = time.time()

            local_path = snapshot_download(
                repo_id=repo_id,
                local_dir=os.path.join(download_dir, size) if download_dir else None,
                tqdm_class=tqdm,
            )

            duration = time.time() - download_start
            tqdm.write(f"  ✓ {size} — {short_path(local_path)} ({duration:.1f}s)")

            model = WhisperModel(local_path, device="cpu", compute_type="int8")
            del model
            gc.collect()

        except Exception as e:
            tqdm.write(f"  ✗ Failed: {size} — {e}")

    total_duration = time.time() - start_time
    print(f"\nAll models processed.")
    print(f"Total time: {total_duration:.1f}s ({total_duration / 60:.1f} min)")

if __name__ == "__main__":
    download_all_faster_whisper_models(download_dir=None)