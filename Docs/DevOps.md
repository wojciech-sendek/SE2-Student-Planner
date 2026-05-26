# DevOps Runbook

## Local Docker Environment

Copy `.env.example` to `.env` if you want to override local ports or credentials. Docker Compose also works with the default values from `docker-compose.yml`.

```bash
docker compose up --build
```

Services:

- Frontend: `http://localhost:5173`
- API: `http://localhost:5289`
- API health: `http://localhost:5289/api/health`
- Mailpit UI: `http://localhost:8025`
- SQL Server: `localhost,1433`

SQL Server data is stored in the `mssql-data` Docker volume, so database contents survive container restarts.

## CI

GitHub Actions runs:

- backend restore, build, and tests
- frontend dependency install with `npm ci`
- frontend lint
- frontend tests
- frontend production build

The frontend Docker image also uses `npm ci`, so local and CI installs are based on the committed `package-lock.json`.

## Current Backend Blocker

The backend currently fails to compile because some files import `StudentPlanner.Api.Entities.Enums`, while the enum types are declared under `StudentPlanner.Api.Entities`. Until that is fixed, backend CI and full `docker compose up --build` will fail at the API image build step.
