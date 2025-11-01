# =========================
# Build aşaması
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala ve restore et
COPY *.csproj .
RUN dotnet restore

# Tüm kaynak dosyaları kopyala ve build et
COPY . .
RUN dotnet publish -c Release -o /out

# =========================
# Run aşaması
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Web uygulamasını başlat
ENTRYPOINT ["dotnet", "DOSSOKAM2019.dll"]
