# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ../MockWebApi/. MockWebApi
COPY ../MockWebApi.Configuration/. ./MockWebApi.Configuration/
RUN dotnet restore ./MockWebApi/MockWebApi.csproj

# Copy everything else and build
RUN dotnet publish -c Release -o out ./MockWebApi/MockWebApi.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MockWebApi.dll"]
