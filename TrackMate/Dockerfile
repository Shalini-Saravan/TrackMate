FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TrackMate/TrackMate.csproj", "TrackMate/"]
COPY ["TrackMate.Models/TrackMate.Models.csproj", "TrackMate.Models/"]
RUN dotnet restore "TrackMate/TrackMate.csproj"
COPY . .
WORKDIR "/src/TrackMate"
RUN dotnet build "TrackMate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrackMate.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TrackMate.dll"]
