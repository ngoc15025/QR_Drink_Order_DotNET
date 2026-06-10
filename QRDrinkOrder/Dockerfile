# =========================================
# STAGE 1: BUILD
# Dùng SDK image để biên dịch source code
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file solution và các file .csproj để restore packages trước
# (Tận dụng Docker cache layer - chỉ restore lại khi có thay đổi dependencies)
COPY QRDrinkOrder.sln ./
COPY QRDrinkOrder.API/QRDrinkOrder.API.csproj QRDrinkOrder.API/
COPY QRDrinkOrder.Shared/QRDrinkOrder.Shared.csproj QRDrinkOrder.Shared/
COPY QRDrinkOrder.Client/QRDrinkOrder.Client.csproj QRDrinkOrder.Client/

RUN dotnet restore QRDrinkOrder.API/QRDrinkOrder.API.csproj

# Copy toàn bộ source code còn lại và publish
COPY . .
RUN dotnet publish QRDrinkOrder.API/QRDrinkOrder.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================================
# STAGE 2: RUNTIME
# Dùng image nhỏ hơn (chỉ runtime, không cần SDK) để chạy
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy các file đã publish từ stage build
COPY --from=build /app/publish .

# Render sử dụng biến môi trường PORT để chỉ định cổng
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "QRDrinkOrder.API.dll"]
