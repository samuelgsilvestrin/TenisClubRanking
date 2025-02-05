# Use SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Copy only the project file first
COPY ["TennisClubRanking.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "TennisClubRanking.csproj" --verbosity normal

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "TennisClubRanking.csproj" -c Release -o /app/build --no-restore --verbosity normal

# Publish the application
RUN dotnet publish "TennisClubRanking.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore --verbosity normal

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TennisClubRanking.dll"]
