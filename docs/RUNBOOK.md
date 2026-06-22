# Runbook

## Backend

When a .NET 9 SDK is available:

1. `cd generated-system/backend/services/platform-app/src/PlatformApp.Api`
2. `dotnet restore`
3. `dotnet run`

The API listens on `http://localhost:5000` when started through `docker-compose` or the configured ASP.NET launch settings.

### Demo users

- `admin / Admin123!`
- `buyer / Buyer123!`

## Frontend

When Node dependencies are installed:

1. `cd generated-system/frontend/web-app`
2. `npm install`
3. `npm run dev`

Set `VITE_API_BASE_URL` if the backend is not hosted on `http://localhost:5000`.

## Containers

From `generated-system/infra`:

1. `docker compose up --build`

This starts:

- the modular-monolith API
- the React frontend
- PostgreSQL 16
- Redis 7
