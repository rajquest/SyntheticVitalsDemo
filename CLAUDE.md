# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

Full-stack healthcare demo app for synthetic vital signs data generation and management.

```
Angular UI (port 4200)
    ↓ HTTP / CORS
ASP.NET Core API (port 5042 / 7000 via Docker)
    ↓ EF Core
MySQL 8.0.46
```

**Backend:** `SyntheticVitalsDemo.Api/` — .NET 10 Web API  
**Frontend:** `SyntheticVitalsDemo.Ui/` — Angular 21 with Angular Material and Chart.js  
**Docker:** `docker-compose.yml` + `docker-compose.override.yml` orchestrate API + MySQL

## Commands

### Backend (.NET API)

```powershell
# Run (development, HTTP)
dotnet run --project SyntheticVitalsDemo.Api/SyntheticVitalsDemo.Api.csproj

# Build
dotnet build

# Add EF migration
dotnet ef migrations add <Name> --project SyntheticVitalsDemo.Api

# Apply migrations
dotnet ef database update --project SyntheticVitalsDemo.Api
```

### Frontend (Angular)

```bash
cd SyntheticVitalsDemo.Ui

npm start          # dev server at http://localhost:4200
npm run build      # production build → dist/
npm run watch      # build with watch mode
npm test           # unit tests via Vitest
```

### Docker (full stack)

```powershell
docker-compose up -d    # API on port 7000, MySQL on 3306
docker-compose down
```

## Backend Structure

**Controllers** (`Controllers/`): `PatientsController`, `VitalsController`, `ClinicsController`, `DashboardController`, `ExportController`, `AdminController`

**Services** (`Services/`):
- `PatientService`, `VitalsService` — CRUD business logic
- `VitalsGenerationService` — synthetic vitals generation using Bogus
- `PulmonaryPressure*Service` — seated/supine pulmonary pressure calculations
- `CsvExportService`, `Hl7ExportService`, `FhirExportService` — multi-format export
- `DemoDataResetService`, `DbSeeder` — demo data lifecycle

**Data** (`Data/`): `AppDbContext` (EF Core), migrations in `Data/Migrations/`

**Models** (`Models/`): `Clinic`, `Patient`, `VitalsSubmission` plus enums `PatientScenario` (16 clinical scenarios), `PulmonaryPressureTrendScenario`, `Sex`

## Frontend Structure

Angular standalone components in `src/app/`:
- `dashboard/` — summary metrics
- `clinic-list/`, `clinic-detail/` — clinic management
- `patient-list/`, `patient-detail/` — patient vitals and history
- `admin-settings/` — demo reset and admin controls

Services in `src/app/services/` proxy all API calls. Charting via Chart.js / ng2-charts.

## Database

- **Connection string** is in `SyntheticVitalsDemo.Api/local.env` (not committed in clean state — check `appsettings.Development.json` or user secrets for overrides)
- `DemoSeed:SeedOnStartup = true` triggers `DbSeeder` on startup to populate clinics and patients
- Core tables: `Clinics`, `Patients`, `VitalsSubmissions`

## Key Domain Concepts

- **PatientScenario** — drives synthetic vitals generation (Normal, Hypertension, HeartFailure variants, LowSpO2, etc.)
- **PulmonaryPressureTrendScenario** — trend direction for pulmonary pressure series (NormalStable, Rising, Falling, Erratic, etc.)
- Vitals include BP (systolic/diastolic), SpO2, HR, weight, and pulmonary arterial pressures in both seated and supine positions
- Series generation supports 7/14/30/60/180/365-day windows via `POST /api/patients/{id}/generate-vitals-series`
- Export formats: CSV, HL7 (via HL7-dotnetcore), FHIR JSON

## Workflow Conventions

- Do not run `dotnet build`, `dotnet test`, `npm test`, or any build/test commands automatically after making changes. The user runs these manually or will explicitly ask.
- Do not suggest or prompt to run builds or tests unless asked.

## CORS & Ports

API allows origins `http://localhost:4200` and `https://localhost:4200`. When running via Docker the API is on port 7000; when running directly via `dotnet run` it uses 5042 (HTTP) or 7067 (HTTPS) per `launchSettings.json`.
