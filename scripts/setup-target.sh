#!/bin/bash
set -e

export DEBIAN_FRONTEND=noninteractive

echo "==> [1/5] Installing Docker..."
apt-get update -qq
apt-get install -y -qq ca-certificates curl gnupg

install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) \
  signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update -qq
apt-get install -y -qq docker-ce docker-ce-cli containerd.io

systemctl enable docker
systemctl start docker

echo "==> [2/5] Installing nginx..."
apt-get install -y -qq nginx

echo "==> [3/5] Configuring nginx..."
cat > /etc/nginx/sites-available/mywebapp << 'EOF'
server {
    listen 80;
    server_name _;

    access_log /var/log/nginx/mywebapp_access.log;
    error_log  /var/log/nginx/mywebapp_error.log;

    location = / {
        proxy_pass         http://127.0.0.1:8000;
        proxy_http_version 1.1;
        proxy_set_header   Host      $host;
        proxy_set_header   X-Real-IP $remote_addr;
    }

    location /tasks {
        proxy_pass         http://127.0.0.1:8000;
        proxy_http_version 1.1;
        proxy_set_header   Host      $host;
        proxy_set_header   X-Real-IP $remote_addr;
    }

    location / {
        return 404;
    }
}
EOF

rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/mywebapp /etc/nginx/sites-enabled/mywebapp
nginx -t
systemctl enable nginx
systemctl start nginx

echo "==> [4/5] Installing PostgreSQL..."
apt-get install -y -qq postgresql

systemctl enable postgresql
systemctl start postgresql

if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_roles WHERE rolname='mywebapp'" | grep -q "1"; then
    sudo -u postgres createuser --no-superuser --no-createdb --no-createrole mywebapp
    sudo -u postgres psql -c "ALTER USER mywebapp WITH PASSWORD 'mywebapp';"
fi
if ! sudo -u postgres psql -lqt | cut -d'|' -f1 | grep -qw mywebapp; then
    sudo -u postgres createdb --owner=mywebapp mywebapp
fi

echo "==> [5/5] Creating deploy user..."
if ! id -u deploy &>/dev/null; then
    useradd --create-home --shell /bin/bash deploy
    usermod -aG docker deploy
fi

mkdir -p /home/deploy/.ssh
chmod 700 /home/deploy/.ssh
touch /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys
chown -R deploy:deploy /home/deploy/.ssh

echo "==> Target setup completed."