#!/bin/bash

echo "🚀 DOSSOKAM Üretim Takip Sistemi Build Ediliyor..."

# Gerekli tool'ları kontrol et
echo "🔍 .NET versiyonu kontrol ediliyor..."
dotnet --version

# Proje dosyasını kontrol et
if [ ! -f "DossokamUretim.csproj" ]; then
    echo "❌ HATA: DossokamUretim.csproj bulunamadı!"
    echo "📁 Mevcut dosyalar:"
    ls -la
    exit 1
fi

# NuGet paketlerini restore et
echo "📦 NuGet paketleri restore ediliyor..."
dotnet restore

# Release modunda build et
echo "🔨 Release build oluşturuluyor..."
dotnet build -c Release --no-restore

# Publish işlemi
echo "📤 Publish işlemi yapılıyor..."
dotnet publish -c Release -o output --nologo

# Output kontrolü
if [ -d "output" ]; then
    echo "✅ Build BAŞARIYLA tamamlandı!"
    echo "📁 Output klasörü içeriği:"
    ls -la output/
    
    # Çalıştırılabilir dosyayı kontrol et
    if [ -f "output/DossokamUretim" ]; then
        echo "🎯 Çalıştırılabilir dosya hazır: output/DossokamUretim"
    else
        echo "🔍 Çalıştırılabilir dosya aranıyor..."
        find output -name "*.dll" -o -name "*.exe" | head -5
    fi
else
    echo "❌ HATA: Output klasörü oluşturulamadı!"
    exit 1
fi

echo "🏁 Build süreci tamamlandı. Render deploy'a hazır!"
