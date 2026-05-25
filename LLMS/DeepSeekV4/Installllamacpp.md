# Create or go to directory Apps
cd ~/apps

or 

mkdir ~/apps

# Clone
git clone https://github.com/antirez/llama.cpp-deepseek-v4-flash.git
git clone https://github.com/cdome94/llama.cpp-deepseek-v4-flash.git

# Build
cmake -B build -DGGML_CUDA=ON -DLLAMA_CURL=ON -DGGML_NATIVE=OFF -DCMAKE_CUDA_ARCHITECTURES="121a-real"  -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release -j 20

# Test build success
ls build/bin/llama-server
ls build/bin/llama-cli

./build/bin/llama-server \
  -m ~/models/deepseek-v4-flash/DeepSeek-V4-Flash-IQ2XXS-w2Q2K-AProjQ8-SExpQ8-OutQ8-chat-v2.gguf \
  -ngl 99 \
  --no-mmap \
  -fa on \
  --jinja \
  --reasoning-format auto \
  -c 32768 \
  -b 1024 \
  -ub 256 \
  -ctk q8_0 \
  -ctv q8_0 \
  --host 0.0.0.0 \
  --port 8080

