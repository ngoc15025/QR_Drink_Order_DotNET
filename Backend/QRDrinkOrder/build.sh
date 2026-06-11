#!/bin/bash
set -e

# 1. Cài đặt .NET 9
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

# 2. Xóa TOÀN BỘ thư mục obj và bin (dùng find để không bị miss)
echo "=== Cleaning obj and bin folders ==="
find . -type d -name "obj" -print -exec rm -rf {} + 2>/dev/null || true
find . -type d -name "bin" -print -exec rm -rf {} + 2>/dev/null || true
echo "=== Done cleaning ==="

# 3. Restore dependencies từ đầu
./dotnet/dotnet restore QRDrinkOrder.Client/QRDrinkOrder.Client.csproj

# 4. Publish
./dotnet/dotnet publish QRDrinkOrder.Client/QRDrinkOrder.Client.csproj -c Release -o output --no-restore
