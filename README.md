# User Directory Application

Single-repo full-stack application for managing user records, built with a React frontend and a .NET 8 backend using Clean Architecture.

## Application Structure

```text
userdirectory/
  backend/
    UserDirectory.sln
    src/
      UserDirectory.Api/            # HTTP API, auth, middleware, Swagger
      UserDirectory.Application/    # Use cases, validation, service contracts
      UserDirectory.Domain/         # Core entities and domain behavior
      UserDirectory.Infrastructure/ # EF Core, SQLite, repositories, seed/init
    tests/
      UserDirectory.Application.Tests/
      UserDirectory.Api.Tests/
  frontend/
    src/
      api/                          # Axios client + API wrappers
      auth/                         # OIDC/Auth0 + local dev auth fallback
      components/
      pages/
      types/
  docker-compose.yml
  README.md
```

## Development Process

1. Implement feature flow from inside out: `Domain` -> `Application` -> `Infrastructure` -> `Api`.
2. Add or update tests in `backend/tests` and `frontend/src/**/__tests__`.
3. Run local services and validate API behavior through Swagger.
4. Run automated tests before merge.
5. Use Docker Compose to validate containerized integration.

### Local Development Loop

Prerequisites:
- .NET SDK 8+
- Node.js 20+
- Docker Desktop (optional, for containerized run)

Backend:

```bash
cd backend
dotnet restore
dotnet run --project src/UserDirectory.Api
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Default local URLs:
- Frontend: `http://localhost:5173`
- Backend (launch profile): `https://localhost:7253` and `http://localhost:5157`
- Swagger: `https://localhost:7253/swagger`

Notes:
- Frontend API base URL is controlled by `VITE_API_BASE_URL` and falls back to `https://localhost:7083` if not set.
- OIDC is enabled when `VITE_AUTH_DOMAIN`, `VITE_AUTH_CLIENT_ID`, and `VITE_AUTH_AUDIENCE` are configured.

### Testing

Backend:

```bash
cd backend
dotnet test UserDirectory.sln
```

Frontend:

```bash
cd frontend
npm test
```

### Containerized Run

```bash
docker compose up --build
```

Container endpoints:
- Frontend: `http://localhost:3000`
- API: `http://localhost:8080`

## Technology Disclosure

Core technologies used in this project:
- Backend: .NET 8, ASP.NET Core Web API, Entity Framework Core, SQLite, FluentValidation, JWT Bearer auth, Swagger/OpenAPI, xUnit, Moq
- Frontend: React 18, TypeScript, Vite, React Router, Axios, Jest, React Testing Library, Auth0 React SDK
- Platform/ops: Docker, Docker Compose, NGINX

## AI Tooling Disclosure

- This README was prepared with AI assistance using GitHub Copilot (model: GPT-5.3-Codex).
- The application itself does not require AI services at runtime.
