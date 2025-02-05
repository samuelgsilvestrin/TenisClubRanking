# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Show SDK info
RUN dotnet --info

# Copy csproj and restore as distinct layers
COPY ["TennisClubRanking.csproj", "./"]
RUN dotnet restore "TennisClubRanking.csproj" --verbosity detailed

# Copy everything else
COPY . .

# Show contents for debugging
RUN ls -la

# Build with detailed output
RUN dotnet build "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    --verbosity detailed

# Publish the application
RUN dotnet publish "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TennisClubRanking.dll"]
