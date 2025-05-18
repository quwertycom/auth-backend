# qAuth API

## 🚀 Quick Start

### Prerequisites
- .NET 8.0
- Docker
- Docker Compose

### Development Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/QUWERTYCOM/auth-backend.git
   cd auth-backend
   ```

2. Set up environment variables:
   ```bash
   cp .env.example .env
   ```
   Edit `.env` with your settings.

### 💻 Development

You have two options for running the application in development:

1. **Full Docker Setup** (recommended):
   ```bash
   # Using VS Code
   Run Task: "🚀 Run Development (Docker)"
   
   # Or using terminal
   docker compose -f docker-compose.dev.yml up --build
   ```

2. **Local API with Docker Database**:
   ```bash
   # Using VS Code
   Run Task: "🚀 Run Development (Local)"
   
   # Or using terminal
   docker compose -f docker-compose.dev.yml up db -d
   cd API && dotnet watch run
   ```

### 🏭 Production

1. Set required environment variables on your host system (or use the defaults in docker-compose.yml):
   - `POSTGRES_USER`
   - `POSTGRES_PASSWORD`
   - `POSTGRES_DB`

2. Run the application:
   ```bash
   # Using VS Code
   Run Task: "🚀 Run Production"
   
   # Or using terminal
   docker compose up --build
   ```

## 🛠️ Available Tasks

VS Code tasks are organized by function:

### Running Applications
- `🚀 Run Development (Docker)` - Full stack in Docker
- `🚀 Run Development (Local)` - API locally with Docker DB
- `🚀 Run Production` - Production setup

### Database Operations
- `🛢️ Start Database` - Start PostgreSQL
- `⏹️ Stop Database` - Stop PostgreSQL

### Maintenance
- `⏹️ Stop All` - Stop all containers
- `🧹 Clean Project` - Clean, restore, and build
- `🧪 Run Tests` - Run test suite

## 📁 Project Structure

```
auth-backend/
├── API/                # Main API project
│   └── Dockerfile      # API-specific Docker build
├── docker-compose.yml  # Production Docker setup
├── docker-compose.dev.yml # Development Docker setup
└── .env.example        # Environment variables template
```

## 🔒 Environment Variables

Required environment variables:
```env
# Database Configuration
POSTGRES_USER=your_username
POSTGRES_PASSWORD=your_password
POSTGRES_DB=your_database

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8000
```

## API Endpoints

The API runs on port 8000. You can check the following endpoints:

- Health check: `http://localhost:8000/health`
- Authentication endpoints: `http://localhost:8000/api/authentication/*`
- Session endpoints: `http://localhost:8000/api/session/*`
- User endpoints: `http://localhost:8000/api/user/*`

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `🧪 Run Tests`
4. Submit a pull request

## 📝 License

[MIT License](LICENSE)

## 👥 Authors

- QUWERTY Team