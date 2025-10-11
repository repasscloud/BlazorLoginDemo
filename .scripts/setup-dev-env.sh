#!/bin/bash

# ----- vars -----
USER=deploy
PUBKEY='ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIFjWFuC84iy2nhO6vqKh9VnrqfipV+l3yheC8vM/k90k danijel@repasscloud.com'

# ----- user + ssh -----
adduser --disabled-password --gecos "" "$USER"
usermod -s /bin/bash "$USER"
install -d -m 700 /home/$USER/.ssh
printf '%s\n' "$PUBKEY" > /home/$USER/.ssh/authorized_keys
chown -R $USER:$USER /home/$USER/.ssh
chmod 600 /home/$USER/.ssh/authorized_keys

# ----- sudo + docker group -----
apt-get update -y
apt-get install -y sudo ca-certificates curl gnupg
groupadd -f docker
usermod -aG sudo,docker "$USER"
printf '%s\n' "$USER ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/90-$USER
chmod 440 /etc/sudoers.d/90-$USER

# ----- install docker engine + compose plugin -----
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
. /etc/os-release
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $VERSION_CODENAME stable" \
  > /etc/apt/sources.list.d/docker.list
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
systemctl enable --now docker

# ensure docker service allows high fds (optional but safe)
mkdir -p /etc/systemd/system/docker.service.d
printf '[Service]\nLimitNOFILE=1048576\n' > /etc/systemd/system/docker.service.d/limits.conf
systemctl daemon-reload
systemctl restart docker

# ----- 2 GB persistent swap -----
fallocate -l 2G /swapfile
chmod 600 /swapfile
mkswap /swapfile
grep -q '^/swapfile ' /etc/fstab || echo '/swapfile none swap sw 0 0' >> /etc/fstab
swapon -a
echo 'vm.swappiness=10' > /etc/sysctl.d/99-swap.conf
sysctl --system >/dev/null

# ----- SSH hardening -----
sed -i 's/^#\?PasswordAuthentication .*/PasswordAuthentication no/' /etc/ssh/sshd_config
sed -i 's/^#\?PermitRootLogin .*/PermitRootLogin no/' /etc/ssh/sshd_config
systemctl restart ssh

# ----- smoke tests -----
docker run --rm hello-world || true
swapon --show

# ----- done -----
echo "Setup complete. You can now SSH into the server as user '$USER'."

