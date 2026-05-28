#!/bin/bash
set -e

export DEBIAN_FRONTEND=noninteractive

REPO_DIR="/vagrant"
APP_DIR="/opt/mywebapp"

echo "==> [1/9] Installing packages..."
apt-get update -qq
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb && rm packages-microsoft-prod.deb
apt-get update -qq
apt-get install -y -qq \
    -o Dpkg::Options::="--force-confdef" \
    -o Dpkg::Options::="--force-confold" \
    postgresql nginx jq dotnet-sdk-10.0

sed -i 's/\r$//' "$REPO_DIR/scripts/migrate.sh"

echo "==> [2/9] Creating users..."

if ! id -u student &>/dev/null; then
    useradd --create-home --shell /bin/bash student
    echo "student:student" | chpasswd
    chage -d 0 student
    usermod -aG sudo student
fi

if ! id -u teacher &>/dev/null; then
    useradd --create-home --shell /bin/bash teacher
    echo "teacher:12345678" | chpasswd
    chage -d 0 teacher
    usermod -aG sudo teacher
fi

if ! id -u app &>/dev/null; then
    useradd --system --no-create-home --shell /usr/sbin/nologin app
fi

if ! id -u operator &>/dev/null; then
    getent group operator >/dev/null || groupadd operator
    useradd --create-home --shell /bin/bash --gid operator operator
    echo "operator:12345678" | chpasswd
    chage -d 0 operator
fi

cat > /etc/sudoers.d/operator << 'EOF'
operator ALL=(ALL) NOPASSWD: /bin/systemctl start mywebapp, \
                              /bin/systemctl stop mywebapp, \
                              /bin/systemctl restart mywebapp, \
                              /bin/systemctl status mywebapp, \
                              /bin/systemctl reload nginx
EOF
chmod 440 /etc/sudoers.d/operator

echo "==> [3/9] Configuring PostgreSQL..."
systemctl start postgresql

PG_VERSION=$(find /etc/postgresql/ -mindepth 1 -maxdepth 1 -type d -printf "%f\n" | head -n 1)
sed -i "s/^#\?listen_addresses.*/listen_addresses = 'localhost'/" \
    "/etc/postgresql/$PG_VERSION/main/postgresql.conf"
systemctl restart postgresql

if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_roles WHERE rolname='mywebapp'" | grep -q 1; then
    sudo -u postgres createuser --no-superuser --no-createdb --no-createrole mywebapp
    sudo -u postgres psql -c "ALTER USER mywebapp WITH PASSWORD 'mywebapp';"
fi
if ! sudo -u postgres psql -lqt | cut -d\| -f1 | grep -qw mywebapp; then
    sudo -u postgres createdb --owner=mywebapp mywebapp
fi

echo "==> [4/9] Building application..."
mkdir -p "$APP_DIR"
dotnet publish "$REPO_DIR/src/mywebapp/mywebapp.csproj" \
    -c Release -r linux-x64 --self-contained false -o "$APP_DIR"
chown -R app:app "$APP_DIR"
chmod +x "$APP_DIR/mywebapp"

echo "==> [5/9] Copying config..."
mkdir -p /etc/mywebapp
cp "$REPO_DIR/config/config.json" /etc/mywebapp/config.json
chown root:app /etc/mywebapp/config.json
chmod 640 /etc/mywebapp/config.json

echo "==> [6/9] Installing systemd service..."

cat > /etc/systemd/system/mywebapp.service << EOF
[Unit]
Description=mywebapp Task Tracker
After=network.target postgresql.service

[Service]
User=app
WorkingDirectory=/opt/mywebapp
ExecStartPre=bash $REPO_DIR/scripts/migrate.sh
ExecStart=/opt/mywebapp/mywebapp
Restart=on-failure

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable mywebapp.service
systemctl restart mywebapp.service

echo "==> [7/9] Configuring nginx..."
cp "$REPO_DIR/nginx/mywebapp.conf" /etc/nginx/sites-available/mywebapp
rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/mywebapp /etc/nginx/sites-enabled/mywebapp
nginx -t
systemctl enable nginx
systemctl reload nginx

echo "==> [8/9] Creating gradebook..."
mkdir -p /home/student
echo "13" > /home/student/gradebook
chown student:student /home/student/gradebook
chmod 644 /home/student/gradebook

echo "==> [9/9] Blocking vagrant user..."
usermod -L vagrant || true

echo ""
echo "==> Installation completed."