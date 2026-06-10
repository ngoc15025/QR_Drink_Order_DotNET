#!/bin/bash

# 1. Cài đặt .NET 9
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

# 2. Xóa các thư mục rác (bin, obj) từ Windows. 
# Lưu ý: Quét từ thư mục cha (..) để xóa rác của cả project Shared.
find .. -type d \( -name "bin" -o -name "obj" \) -prune -exec rm -rf {} +

# 3. Build project thẳng tiến (vì đã đứng sẵn ở thư mục Client)
./dotnet/dotnet publish QRDrinkOrder.Client.csproj -c Release -o output
