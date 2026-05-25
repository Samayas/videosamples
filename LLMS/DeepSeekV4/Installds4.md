# Create or go to directory Apps
cd ~/apps

or 

mkdir ~/apps

# Clone
git clone https://github.com/antirez/ds4.git

# Build
make cuda-spark

# Cli run
cd ~/ds4

./ds4 \
  --model ~/models/deepseek-v4-flash/DeepSeek-V4-Flash-IQ2XXS-w2Q2K-AProjQ8-SExpQ8-OutQ8-chat-v2.gguf

# server run
cd ~/ds4

./ds4 \
  --server \
  --host 0.0.0.0 \
  --port 1234 \
  --model ~/models/deepseek-v4-flash/DeepSeek-V4-Flash-IQ2XXS-w2Q2K-AProjQ8-SExpQ8-OutQ8-chat-v2.gguf

# Start Service
systemctl --user daemon-reload
systemctl --user enable ds4.service
systemctl --user start ds4.service
systemctl --user status ds4.service
