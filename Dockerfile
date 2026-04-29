# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# copia apenas o necessário primeiro (melhora cache)
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/out --no-restore

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# FFmpeg leve + limpeza agressiva
RUN apt-get update \
 && apt-get install -y ffmpeg \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "AquaAI.dll"]