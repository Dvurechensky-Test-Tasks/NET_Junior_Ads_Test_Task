FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Скопировать только csproj
COPY ["AdRegionService.csproj", "./"]

# Restore по правильному пути
RUN dotnet restore "./AdRegionService.csproj"

# Скопировать всё остальное
COPY . .

# Build
RUN dotnet build "./AdRegionService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AdRegionService.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5411

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5411

ENTRYPOINT ["dotnet", "AdRegionService.dll"]
