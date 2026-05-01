# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/out --no-restore

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# INSTALA FFMPEG
RUN apt-get update && apt-get install -y ffmpeg

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "AquaAI.dll"]