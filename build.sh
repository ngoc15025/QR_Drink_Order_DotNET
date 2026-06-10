#!/bin/bash

# Tải và cài đặt .NET 9 SDK vào thư mục cục bộ ./dotnet9
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

# Hiển thị phiên bản để kiểm tra
./dotnet/dotnet --version

# Publish Frontend (Blazor WebAssembly) ra thư mục output
./dotnet/dotnet publish Backend/QRDrinkOrder/QRDrinkOrder.Client/QRDrinkOrder.Client.csproj -c Release -o output
