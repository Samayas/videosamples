# lmstudio.desktop

this file needs to be copied to \~/.config/systemd/user

# enable
systemctl --user daemon-reload
systemctl --user enable lmstudio.service

# Stop / Start
systemctl --user stop lms.service
systemctl --user start lms.service
systemctl --user status lms.service