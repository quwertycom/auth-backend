# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy only the project file first to leverage Docker cache
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "API/API.csproj" \
    --no-cache

# Copy the rest of the files and build
COPY . .
RUN dotnet build "API/API.csproj" -c Release -o /app/build \
    && dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app

# Add non-root user for security
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app

# Copy only the published files from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Configure environment
ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80

# Switch to non-root user
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]

# Add labels for better maintainability
LABEL maintainer="QUWERTY" \
      description="qAuth API Backend" \
      version="1.0" 