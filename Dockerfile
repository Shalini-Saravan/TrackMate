#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y gss-ntlmssp
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TrackMate.Api/TrackMate.Api.csproj", "TrackMate.Api/"]
COPY ["TrackMate.Models/TrackMate.Models.csproj", "TrackMate.Models/"]
RUN dotnet restore "TrackMate.Api/TrackMate.Api.csproj"
COPY . .
WORKDIR "/src/TrackMate.Api"
RUN dotnet build "TrackMate.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrackMate.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrackMate.Api.dll"]