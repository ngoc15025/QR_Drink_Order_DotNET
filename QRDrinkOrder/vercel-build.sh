#!/bin/bash
# vercel-build.sh
# Script tự động hóa quy trình build Blazor WebAssembly (.NET 9) trên Vercel
# Giải quyết triệt để giới hạn 256 ký tự của trường Build Command trên Vercel Dashboard.

set -e

echo "=== 1. Cài đặt .NET 9 SDK ==="
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- -c 9.0 -InstallDir ~/dotnet
export PATH=~/dotnet:$PATH

echo "=== 2. Tạo appsettings.Production.json từ biến môi trường Vercel ==="
cat <<EOF > QRDrinkOrder.Client/wwwroot/appsettings.Production.json
{
  "BackendApiUrl": "${BACKEND_API_URL:-https://qr-drink-order-dotnet.onrender.com}",
  "Firebase": {
    "ApiKey": "${FIREBASE_API_KEY}",
    "AuthDomain": "${FIREBASE_AUTH_DOMAIN}",
    "ProjectId": "${FIREBASE_PROJECT_ID}",
    "StorageBucket": "${FIREBASE_STORAGE_BUCKET}",
    "MessagingSenderId": "${FIREBASE_MESSAGING_SENDER_ID}",
    "AppId": "${FIREBASE_APP_ID}"
  }
}
EOF

echo "=== 3. Publish dự án QRDrinkOrder.Client ==="
dotnet publish QRDrinkOrder.Client/QRDrinkOrder.Client.csproj -c Release -o output

echo "=== Build hoàn tất thành công! ==="
