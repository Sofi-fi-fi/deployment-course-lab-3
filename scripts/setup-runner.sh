#!/bin/bash
set -e

GITHUB_REPO="https://github.com/Sofi-fi-fi/deployment-course-lab-3"
RUNNER_TOKEN="BNM273DSEQRCCQKOEVJSZWTKDNP6W"
RUNNER_VERSION="2.321.0"

echo "==> [1/4] Installing dependencies..."
sudo apt-get update -qq
sudo apt-get install -y -qq curl jq openssh-client

echo "==> [2/4] Downloading GitHub Actions runner..."
mkdir -p /home/vagrant/actions-runner
cd /home/vagrant/actions-runner

curl -sL "https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz" \
  -o runner.tar.gz
tar xzf runner.tar.gz
rm runner.tar.gz

echo "==> [3/4] Configuring runner..."
./config.sh \
  --url "$GITHUB_REPO" \
  --token "$RUNNER_TOKEN" \
  --name "lab3-runner" \
  --labels "lab3,self-hosted" \
  --unattended \
  --replace

echo "==> [4/4] Installing runner as systemd service..."
sudo ./svc.sh install vagrant
sudo ./svc.sh start

echo "==> Runner setup completed."