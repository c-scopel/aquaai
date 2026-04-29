FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app
COPY . .

RUN apt-get update && apt-get install -y ffmpeg

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /app/out .

RUN apt-get update && apt-get install -y ffmpeg

ENTRYPOINT ["dotnet", "AquaAI.dll"]