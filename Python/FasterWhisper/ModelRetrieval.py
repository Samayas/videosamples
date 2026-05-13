# pip install faster-whisper   
# pip install tqdm
# pip install huggingface_hub
# run once to install the package

import sys
from faster_whisper import WhisperModel
from huggingface_hub import utils
from huggingface_hub.utils import tqdm as hf_tqdm

hf_tqdm.tqdm = lambda *args, **kwargs: __import__('tqdm').tqdm(*args, **{**kwargs, 'file': sys.stdout})

utils.enable_progress_bars()

# Choose model size and compute device.
#   - Model :         base / tiny / small / medium / large-v2 / large-v3
#   - For CPU only:   device="cpu", compute_type="int8"
#   - For GPU (CUDA): device="cuda", compute_type="float16"  (or "int8_float16")
model_name = "base"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

# Transcribe – returns a generator of segments and language info.
segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")

model_name = "tiny"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")

model_name = "small"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")

model_name = "medium"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments: 
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")

model_name = "large-v2"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")

model_name = "large-v3"
model = WhisperModel(model_name, device="cpu", compute_type="int8", download_root=None)

segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - CPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")


model_name = "base"
model = WhisperModel(model_name, device="cuda", compute_type="int8_float16", download_root=None)

# Transcribe – returns a generator of segments and language info.
segments, info = model.transcribe("Recording.wav", beam_size=5)

print(f"Model: {model_name} - GPU - Detected language: {info.language} (probability {info.language_probability:.2f})")
for segment in segments:
    print(f"[{segment.start:.2f}s → {segment.end:.2f}s] {segment.text}")
