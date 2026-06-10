#!/bin/bash

# 1. Cài đặt .NET 9
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

# 2. Xóa DỨT ĐIỂM các file rác bằng nhiều cách để đảm bảo 100% sạch sẽ
rm -rf */obj */bin
rm -rf obj bin
./dotnet/dotnet clean QRDrinkOrder.Client/QRDrinkOrder.Client.csproj

# 3. Build project
./dotnet/dotnet publish QRDrinkOrder.Client/QRDrinkOrder.Client.csproj -c Release -o output
