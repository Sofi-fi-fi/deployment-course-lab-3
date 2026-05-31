#!/bin/bash
set -e

TARGET_HOST="${1:-192.168.56.20}"
BASE_URL="http://${TARGET_HOST}"

echo "==> Starting verification against ${BASE_URL}"

check() {
    local description="$1"
    local expected_code="$2"
    local url="$3"

    actual_code=$(curl -s -H "Accept: text/html" -o /dev/null -w "%{http_code}" --max-time 10 "$url")

    if [ "$actual_code" -eq "$expected_code" ]; then
        echo "  [OK] ${description} — HTTP ${actual_code}"
    else
        echo "  [FAIL] ${description} — expected HTTP ${expected_code}, got HTTP ${actual_code}"
        exit 1
    fi
}

echo ""
echo "--- Checking main endpoints ---"
check "GET / returns HTML page"      200 "${BASE_URL}/"
check "GET /tasks returns task list" 200 "${BASE_URL}/tasks"

echo ""
echo "--- Checking nginx blocks health endpoints ---"
check "GET /health/alive returns 404" 404 "${BASE_URL}/health/alive"
check "GET /health/ready returns 404" 404 "${BASE_URL}/health/ready"

echo ""
echo "--- Checking task creation ---"
response=$(curl -s -X POST "${BASE_URL}/tasks" \
    -H "Content-Type: application/json" \
    -d '{"title": "Verification task"}' \
    --max-time 10)

if echo "$response" | grep -q "Verification task"; then
    echo "  [OK] POST /tasks creates task successfully"
else
    echo "  [FAIL] POST /tasks — unexpected response: ${response}"
    exit 1
fi

echo ""
echo "==> All checks passed!"