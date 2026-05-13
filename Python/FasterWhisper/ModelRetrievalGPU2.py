import os
import sys
import ctypes
from pathlib import Path

def setup_cuda() -> None:
    python_base_path = Path(sys.executable).parent.parent
    nvidia_base_path = python_base_path / "Lib" / "site-packages" / "nvidia"

    cuda_runtime_bin_path = nvidia_base_path / "cuda_runtime" / "bin"
    cuda_runtime_lib_path = nvidia_base_path / "cuda_runtime" / "lib" / "x64"
    cublas_bin_path = nvidia_base_path / "cublas" / "bin"
    cudnn_bin_path = nvidia_base_path / "cudnn" / "bin"
    nvrtc_bin_path = nvidia_base_path / "cuda_nvrtc" / "bin"

    paths_to_add = [
        cuda_runtime_bin_path,
        cuda_runtime_lib_path,
        cublas_bin_path,
        cudnn_bin_path,
        nvrtc_bin_path,
    ]

    existing_path = os.environ.get("PATH", "")
    valid_paths = []

    for path_item in paths_to_add:
        if path_item.exists():
            path_text = str(path_item)
            valid_paths.append(path_text)
            os.add_dll_directory(path_text)

    os.environ["PATH"] = os.pathsep.join(valid_paths + [existing_path])
    os.environ["CUDA_PATH"] = str(nvidia_base_path / "cuda_runtime")

    dlls_to_preload = [
        cublas_bin_path / "cublasLt64_12.dll",
        cublas_bin_path / "cublas64_12.dll",
        cuda_runtime_bin_path / "cudart64_12.dll",
        cudnn_bin_path / "cudnn64_9.dll",
        cudnn_bin_path / "cudnn_ops64_9.dll",
        cudnn_bin_path / "cudnn_cnn64_9.dll",
    ]

    for dll_path in dlls_to_preload:
        if dll_path.exists():
            ctypes.WinDLL(str(dll_path))

setup_cuda()

from faster_whisper import WhisperModel

model = WhisperModel("small", device="cuda", compute_type="float16")
segments, info = model.transcribe("Recording.wav", beam_size=5)

print(info.language, info.language_probability)
for segment in segments:
    print(f"[{segment.start:.2f} -> {segment.end:.2f}] {segment.text}")