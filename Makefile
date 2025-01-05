.PHONY: dev build run clean restore test watch

# Docker commands
dev:
	docker compose -f docker-compose.dev.yml up

build:
	docker compose build

run:
	docker compose up -d

clean:
	docker compose down
	docker compose -f docker-compose.dev.yml down
	rm -rf API/bin API/obj

# .NET commands
restore:
	dotnet restore API/API.csproj

test:
	dotnet test

watch:
	dotnet watch --project API/API.csproj run

# Combined commands
rebuild: clean restore build run