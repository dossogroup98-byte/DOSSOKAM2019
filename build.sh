#!/bin/bash
set -e

echo "🔍 .NET ortamı kontrol ediliyor..."
curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet

export DOTNET_ROOT=./dotnet
export PATH=$DOTNET_ROOT:$PATH

echo "📦 .NET paketleri yükleniyor..."
dotnet restore

echo "🔨 Build işlemi..."
dotnet build -c Release --no-restore

echo "📤 Publish işlemi..."
dotnet publish -c Release -o output /p:UseAppHost=true

echo "✅ Build tamamlandı!"
ls -la output/
