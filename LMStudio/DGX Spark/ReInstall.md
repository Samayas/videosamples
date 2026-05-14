* Install Script for LM Studio Desktop on Ubuntu
    cd ~/apps/lmstudio
    sudo rm -r *
    cp Downloads/LM\*.AppImage \~/apps/lmstudio
    chmod +x LM\*.AppImage
    ./LM-Studio-0.4.12-a-arm64.AppImage --appimage-extract
    mv squashfs-root/\* .
    mv squashfs-root/.\[!.]\* . 2> /dev/null ||true
    rmdir squashfs-root
    sudo chown root:root chrome-sandbox
    sudo chmod 4755 chrome-sandbox
    rm LM\*.AppImage
