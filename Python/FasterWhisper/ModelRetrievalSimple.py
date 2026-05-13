# pip install faster-whisper   
# pip install tqdm
# pip install huggingface_hub
# run once to install the package

import os
import sys
from pathlib import Path

os.environ["OMP_NUM_THREADS"] = "16"

from faster_whisper import WhisperModel
from huggingface_hub import utils
from huggingface_hub.utils import tqdm as hf_tqdm

hf_tqdm.tqdm = lambda *args, **kwargs: __import__('tqdm').tqdm(*args, **{**kwargs, 'file': sys.stdout})

utils.enable_progress_bars()

audio_path = Path(r"D:\Projects\Samayas\Videos\122 LM Studio\Video\122 LM Studio-Youtube.mp4")
output_path = Path(r"D:\Projects\Samayas\Videos\122 LM Studio\transcribe.txt")

if not audio_path.exists():
    raise FileNotFoundError(f"File not found: {audio_path}")

# Choose model size and compute device.
#   - Model :         base / tiny / small / medium / large-v2 / large-v3
#   - For CPU only:   device="cpu", compute_type="int8"
#   - For GPU (CUDA): device="cuda", compute_type="float16"  (or "int8_float16")
model_name = "medium"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None, cpu_threads=16)

segments, info = model.transcribe(str(audio_path), beam_size=5)


with output_path.open("w", encoding="utf-8") as text_file:
    header = (
        f"Model: {model_name} - CPU - Detected language: "
        f"{info.language} (probability {info.language_probability:.2f})"
    )
    print(header)
    text_file.write(header + "\n\n")

    for segment in segments:
        line = f"[{segment.start:.2f}s -> {segment.end:.2f}s] {segment.text}"
        print(line)
        text_file.write(line + "\n")
