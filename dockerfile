# .NET 8 SDK (build aşaması)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

# .NET 8 runtime (çalışma aşaması)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "DOSSOKAM-3.dll"]
