# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "API/API.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet build "API/API.csproj" -c Release -o /app/build
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Development stage (new)
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS development
WORKDIR /src
COPY . .
CMD ["dotnet", "watch", "--project", "API/API.csproj", "run", "--no-restore"]

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Add non-root user for security
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app

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