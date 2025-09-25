# Quit Docker if still around
osascript -e 'tell application "Docker" to quit' >/dev/null 2>&1 || true
pkill -f '/Applications/Docker.app' >/dev/null 2>&1 || true

# Unload helper
sudo launchctl unload -w /Library/LaunchDaemons/com.docker.vmnetd.plist 2>/dev/null || true
sudo rm -f /Library/LaunchDaemons/com.docker.vmnetd.plist /Library/PrivilegedHelperTools/com.docker.vmnetd 2>/dev/null || true

# Clear immutable flags/xattrs just in case
chflags -R nouchg,noschg ~/Library/Containers/com.docker.docker 2>/dev/null || true
xattr -cr ~/Library/Containers/com.docker.docker 2>/dev/null || true

# Delete the sandboxed container data (requires Full Disk Access)
rm -rf ~/Library/Containers/com.docker.docker
rm -rf ~/Library/Group\ Containers/group.com.docker
rm -rf ~/Library/Application\ Support/Docker\ Desktop \
       ~/Library/Caches/com.docker.docker \
       ~/Library/Logs/Docker\ Desktop \
       ~/Library/Preferences/com.docker.docker.plist \
       ~/Library/Preferences/com.electron.docker-frontend.plist \
       ~/Library/Saved\ Application\ State/com.electron.docker-frontend.savedState

# CLI + per-user config
for pfx in /usr/local /opt/homebrew ; do
  sudo rm -f  $pfx/bin/docker $pfx/bin/dockerd $pfx/bin/docker-compose \
               $pfx/bin/docker-buildx $pfx/bin/docker-credential-desktop \
               $pfx/bin/com.docker.cli 2>/dev/null || true
  sudo rm -rf $pfx/lib/docker/cli-plugins 2>/dev/null || true
done
rm -rf ~/.docker ~/.docker/cli-plugins 2>/dev/null || true

# Any leftover raw disk images
find ~/Library/Containers -maxdepth 3 -type f \( -name 'Docker.raw' -o -name 'Docker.qcow2' \) -print -delete 2>/dev/null || true

# Socket shim
sudo rm -f /var/run/docker.sock 2>/dev/null || true
