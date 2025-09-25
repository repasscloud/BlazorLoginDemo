#!/usr/bin/env zsh
# nuke-docker-macos.zsh — completely remove Docker Desktop + data on macOS
set -euo pipefail

[[ "$(uname -s)" == "Darwin" ]] || { echo "This script is for macOS only."; exit 1; }

echo "This will COMPLETELY remove Docker Desktop, CLI shims, images/volumes/networks, and config."
read "ok?Proceed? [y/N]: " ; [[ "${ok:l}" == "y" ]] || { echo "Aborted."; exit 1; }

# Ask for sudo once
if ! sudo -v; then echo "sudo required"; exit 1; fi
# Keep sudo alive while we run
while true; do sudo -n true; sleep 60; kill -0 $$ || exit; done 2>/dev/null &

echo "→ Trying to gracefully prune Docker data (if daemon is up)…"
if command -v docker >/dev/null 2>&1; then
  # Try to stop running containers first
  if docker info >/dev/null 2>&1; then
    docker ps -q | xargs -r docker stop || true
    docker system prune -a --volumes -f || true
    # Remove contexts (including docker-desktop)
    docker context ls --format '{{.Name}}' 2>/dev/null | xargs -r -n1 docker context rm -f || true
  fi
fi

echo "→ Quitting Docker Desktop app…"
osascript -e 'tell application "Docker" to quit' >/dev/null 2>&1 || true
# Kill any stragglers
pkill -f '/Applications/Docker.app'             >/dev/null 2>&1 || true
pkill -f 'com.docker.backend'                  >/dev/null 2>&1 || true
pkill -f 'vpnkit'                              >/dev/null 2>&1 || true
pkill -f 'dockerd'                             >/dev/null 2>&1 || true

echo "→ Unloading privileged helper (vmnetd)…"
sudo launchctl unload -w /Library/LaunchDaemons/com.docker.vmnetd.plist  >/dev/null 2>&1 || true

echo "→ Removing Docker Desktop app and helpers…"
sudo rm -rf /Applications/Docker.app
sudo rm -f  /Library/LaunchDaemons/com.docker.vmnetd.plist
sudo rm -f  /Library/PrivilegedHelperTools/com.docker.vmnetd

echo "→ Removing user-level Docker data…"
rm -rf  "$HOME/Library/Containers/com.docker.docker"
rm -rf  "$HOME/Library/Containers/com.docker.helper"             2>/dev/null || true
rm -rf  "$HOME/Library/Group Containers/group.com.docker"
rm -rf  "$HOME/Library/Application Support/Docker Desktop"
rm -rf  "$HOME/Library/Caches/com.docker.docker"
rm -rf  "$HOME/Library/Logs/Docker Desktop"
rm -rf  "$HOME/Library/Preferences/com.docker.docker.plist"
rm -rf  "$HOME/Library/Preferences/com.electron.docker-frontend.plist"
rm -rf  "$HOME/Library/Saved Application State/com.electron.docker-frontend.savedState"
# Socket shim (if present)
sudo rm -f /var/run/docker.sock 2>/dev/null || true

echo "→ Removing CLI shims / plugins (Intel + Apple Silicon paths)…"
for pfx in /usr/local /opt/homebrew ; do
  sudo rm -f  $pfx/bin/docker $pfx/bin/dockerd $pfx/bin/docker-compose \
               $pfx/bin/docker-buildx $pfx/bin/docker-credential-desktop \
               $pfx/bin/com.docker.cli 2>/dev/null || true
  sudo rm -rf $pfx/lib/docker/cli-plugins 2>/dev/null || true
done
rm -rf "$HOME/.docker/cli-plugins" 2>/dev/null || true

echo "→ Removing per-user Docker config (contexts, auth, etc.)…"
rm -rf "$HOME/.docker"

echo "→ Double-checking leftover raw VM disks…"
# Older/newer locations (we already removed Container dir, this is just in case)
find "$HOME/Library/Containers" -maxdepth 3 -type f -name 'Docker.raw' -print -delete 2>/dev/null || true
find "$HOME/Library/Containers" -maxdepth 3 -type f -name 'Docker.qcow2' -print -delete 2>/dev/null || true

echo "→ Removing any registry credential helpers left by Desktop…"
for bin in docker-credential-desktop docker-credential-osxkeychain ; do
  which $bin >/dev/null 2>&1 && sudo rm -f "$(which $bin)" || true
done

echo "→ If installed via Homebrew, uninstall cask + formulae (best-effort)…"
if command -v brew >/dev/null 2>&1; then
  brew list --cask 2>/dev/null | grep -q '^docker$' && brew uninstall --cask docker || true
  # These may or may not exist
  for f in docker docker-compose docker-buildx; do
    brew list --formula 2>/dev/null | grep -q "^${f}$" && brew uninstall "$f" || true
  done
fi

echo "→ Flushing macOS LaunchServices & cache (best-effort)…"
/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister \
  -f /System/Library/CoreServices/Finder.app >/dev/null 2>&1 || true

echo "→ Done. Quick verification:"
set +e
command -v docker && echo "   ⚠️  docker still on PATH -> remove manually" || echo "   ✅ docker CLI removed"
[[ -e /var/run/docker.sock ]] && echo "   ⚠️  /var/run/docker.sock exists" || echo "   ✅ no docker.sock"
echo "   You may want to reboot to clear any kext/network leftovers."
