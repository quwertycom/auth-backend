# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["API.csproj", "./"]
RUN dotnet restore

# Copy the rest of the project and build
COPY . .
RUN dotnet build "API.csproj" -c Release -o /app/build
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser:appuser /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

# Expose the HTTP port
EXPOSE 8000

# Switch to non-root user
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]

# Add labels for better maintainability
LABEL maintainer="QUWERTY" \
      description="qAuth API Backend" \
      version="1.0" \
      org.opencontainers.image.source="https://github.com/QUWERTYCOM/auth-backend" 