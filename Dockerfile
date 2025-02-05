# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0.101-bookworm-slim AS build

# Set working directory
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["TennisClubRanking.csproj", "./"]
RUN dotnet restore "TennisClubRanking.csproj"

# Copy everything else
COPY . .

# Build with detailed output
RUN dotnet build "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    -p:PublishReadyToRun=false

# Publish the application
RUN dotnet publish "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0.1-bookworm-slim
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TennisClubRanking.dll"]
