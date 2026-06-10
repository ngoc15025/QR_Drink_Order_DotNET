#!/bin/bash

# 1. Cài đặt .NET 9
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

# 2. Xóa sạch các thư mục rác (bin, obj) từ Windows
find . -type d \( -name "bin" -o -name "obj" \) -prune -exec rm -rf {} +

# 3. Build project
./dotnet/dotnet publish QRDrinkOrder.Client/QRDrinkOrder.Client.csproj -c Release -o output
