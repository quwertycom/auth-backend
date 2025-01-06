# qAuth API

## Environment Setup

### Development
1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```
2. Modify the values in `.env` according to your needs

### Production
For production deployment, set the following environment variables on your host system:
- `POSTGRES_USER`: Database username
- `POSTGRES_PASSWORD`: Database password
- `POSTGRES_DB`: Database name

Do not commit the `.env` file to source control.

## Running the Application

### Local Development
1. Start the database and application:
   ```bash
   # Using VS Code
   Run task: "(dev) run app - local"
   ```
   Or manually:
   ```bash
   # Start database
   docker compose -f docker-compose.dev.yml up db -d
   
   # Start application
   cd API && dotnet watch run
   ```

### Production
1. Set the required environment variables
2. Run the application:
   ```bash
   docker compose up -d
   ```

## Development Tools
- .NET 8.0
- PostgreSQL 15
- Docker