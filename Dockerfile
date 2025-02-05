# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["TennisClubRanking.csproj", "./"]
RUN dotnet restore "TennisClubRanking.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "TennisClubRanking.csproj" -c Release -o /app/build --no-restore

# Publish the application
RUN dotnet publish "TennisClubRanking.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TennisClubRanking.dll"]
