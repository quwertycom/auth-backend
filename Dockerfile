# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only solution and project files first
COPY ["Auth.sln", "."]
COPY ["API/API.csproj", "API/"]
COPY ["Tests/API.UnitTests/API.UnitTests.csproj", "Tests/API.UnitTests/"]
COPY ["Tests/API.IntegrationTests/API.IntegrationTests.csproj", "Tests/API.IntegrationTests/"]
COPY ["Tests/API.FunctionalTests/API.FunctionalTests.csproj", "Tests/API.FunctionalTests/"]
COPY ["Tests/API.PerformanceTests/API.PerformanceTests.csproj", "Tests/API.PerformanceTests/"]

# Restore NuGet packages
RUN dotnet restore "Auth.sln"

# Copy the rest of the files
COPY . .

# Build and publish
RUN dotnet build "API/API.csproj" -c Release -o /app/build
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Development stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

# Copy solution and project files
COPY ["Auth.sln", "."]
COPY ["API/API.csproj", "API/"]
COPY ["Tests/API.UnitTests/API.UnitTests.csproj", "Tests/API.UnitTests/"]
COPY ["Tests/API.IntegrationTests/API.IntegrationTests.csproj", "Tests/API.IntegrationTests/"]
COPY ["Tests/API.FunctionalTests/API.FunctionalTests.csproj", "Tests/API.FunctionalTests/"]
COPY ["Tests/API.PerformanceTests/API.PerformanceTests.csproj", "Tests/API.PerformanceTests/"]

# Restore packages in the correct location
RUN dotnet restore "Auth.sln"

# Copy everything else
COPY . .

# Build the project in Debug mode to ensure all dependencies are available
RUN dotnet build "API/API.csproj" -c Debug

# Set working directory to the API project
WORKDIR /app/API

# Set environment variables
ENV DOTNET_USE_POLLING_FILE_WATCHER=1 \
    ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_Kestrel__EndpointDefaults__Protocols=Http1

ENV DOCKER_RUNNING=true

# Expose the port
EXPOSE 80

# Set the entry point for development
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--urls", "http://+:80"]

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Add non-root user for security
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app

# Configure environment
ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

ENV DOCKER_RUNNING=true

# Expose port
EXPOSE 80

# Switch to non-root user
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]

# Add labels for better maintainability
LABEL maintainer="QUWERTY" \
      description="qAuth API Backend" \
      version="1.0" \
      org.opencontainers.image.source="https://github.com/QUWERTYCOM/auth-backend" 