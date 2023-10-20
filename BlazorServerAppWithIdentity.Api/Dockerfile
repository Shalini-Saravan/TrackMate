#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BlazorServerAppWithIdentity.Api/BlazorServerAppWithIdentity.Api.csproj", "BlazorServerAppWithIdentity.Api/"]
COPY ["BlazorServerAppWithIdentity.Models/BlazorServerAppWithIdentity.Models.csproj", "BlazorServerAppWithIdentity.Models/"]
RUN dotnet restore "BlazorServerAppWithIdentity.Api/BlazorServerAppWithIdentity.Api.csproj"
COPY . .
WORKDIR "/src/BlazorServerAppWithIdentity.Api"
RUN dotnet build "BlazorServerAppWithIdentity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorServerAppWithIdentity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorServerAppWithIdentity.Api.dll"]