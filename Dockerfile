# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Show dotnet info for debugging
RUN dotnet --info

# Copy only the project file first
COPY ["TennisClubRanking.csproj", "./"]

# Show project file contents for debugging
RUN cat TennisClubRanking.csproj

# Restore NuGet packages with detailed logging
RUN dotnet restore "TennisClubRanking.csproj" --verbosity detailed

# Copy the rest of the source code
COPY . .

# List contents for debugging
RUN ls -la

# Build with detailed logging
RUN dotnet build "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    --verbosity detailed \
    -o /app/build \
    /p:GenerateFullPaths=true \
    /consoleloggerparameters:NoSummary

# Publish the application
RUN dotnet publish "TennisClubRanking.csproj" \
    --configuration Release \
    --no-restore \
    -o /app/publish \
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
