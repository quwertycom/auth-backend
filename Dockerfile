# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only solution and project files first
COPY ["Auth.sln", "."]
COPY ["API/API.csproj", "API/"]
COPY ["Tests/API.Tests.Unit/API.Tests.Unit.csproj", "Tests/API.Tests.Unit/"]
COPY ["Tests/API.Tests.Integration/API.Tests.Integration.csproj", "Tests/API.Tests.Integration/"]
COPY ["Tests/API.Tests.Functional/API.Tests.Functional.csproj", "Tests/API.Tests.Functional/"]
COPY ["Tests/API.Tests.DataGenerator/API.Tests.DataGenerator.csproj", "Tests/API.Tests.DataGenerator/"]

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
COPY ["Tests/API.Tests.Unit/API.Tests.Unit.csproj", "Tests/API.Tests.Unit/"]
COPY ["Tests/API.Tests.Integration/API.Tests.Integration.csproj", "Tests/API.Tests.Integration/"]
COPY ["Tests/API.Tests.Functional/API.Tests.Functional.csproj", "Tests/API.Tests.Functional/"]
COPY ["Tests/API.Tests.DataGenerator/API.Tests.DataGenerator.csproj", "Tests/API.Tests.DataGenerator/"]

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
    ASPNETCORE_ENVIRONMENT=DockerDevelopment \
    ASPNETCORE_URLS=http://0.0.0.0:8000 \
    ASPNETCORE_Kestrel__EndpointDefaults__Protocols=Http1 \
    DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1 \
    DOTNET_WATCH_SUPPRESS_PROMPTS=1

ENV DOCKER_RUNNING=true

# Expose the port
EXPOSE 8000

# Set the entry point for development
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--urls", "http://0.0.0.0:8000", "--environment", "DockerDevelopment"]

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Create directory for certificates
RUN mkdir -p /https && \
    adduser --disabled-password --gecos "" appuser && \
    chown -R appuser:appuser /app && \
    chown -R appuser:appuser /https

# Configure environment
ENV ASPNETCORE_URLS=http://+:80;https://+:443 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword123!

ENV DOCKER_RUNNING=true

# Expose ports
EXPOSE 80
EXPOSE 443

# Switch to non-root user
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]

# Add labels for better maintainability
LABEL maintainer="QUWERTY" \
      description="qAuth API Backend" \
      version="1.0" \
      org.opencontainers.image.source="https://github.com/QUWERTYCOM/auth-backend" 