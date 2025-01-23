# Clinical Trial Management API

A modern .NET API for managing clinical trial metadata, built with clean architecture principles and modern development practices.

## Features

- Upload and validate clinical trial data in JSON format
- Schema-based validation for trial data
- RESTful endpoints for trial management
- PostgreSQL database for data persistence
- Event-driven architecture for extensibility
- Integration tests with test containers

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop (for PostgreSQL you can use the local postgres installation instead of docker)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd clinical-trial-app
```

### 2. Database Setup

The application uses PostgreSQL. You have two options:

#### Option A: Using Docker (Recommended)
```bash
docker run --name clinical-trial-db \
  -e POSTGRES_DB=clinicalTrial \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:latest
```

#### Option B: Local PostgreSQL Installation
- Install PostgreSQL
- Create a database named `clinicalTrial`
- Update connection string in `appsettings.json` if needed

### 3. Configuration

The default configuration in `appsettings.json` should work out of the box with the Docker setup. If you need to modify it:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=clinicalTrial;Username=postgres;Password=postgres"
  }
}
```

### 4. Run the Application

```bash
dotnet restore
dotnet build
dotnet run --project ClinicalTrialApp
```

The API will be available at:
- HTTP: http://localhost:5177
- HTTPS: https://localhost:7177

Swagger UI will be available at: https://localhost:7177/swagger

## API Endpoints

### Upload Trial Data
```http
POST /api/trials/upload
Content-Type: multipart/form-data

file=@trial.json
```

Example trial.json:
```json
{
    "trialId": "TRIAL-001",
    "title": "Example Clinical Trial",
    "startDate": "2024-01-25",
    "endDate": "2024-02-25",
    "participants": 100,
    "status": "Ongoing"
}
```

### Get Trial by ID
```http
GET /api/trials/{id}
```

### List All Trials
```http
GET /api/trials
```

Optional query parameter for filtering by status:
```http
GET /api/trials?status=Ongoing
```

## Running Tests

```bash
dotnet test
```

The test suite includes:
- Unit tests for core business logic
- Integration tests using test containers
- Validation tests for JSON schema

## Schema Validation

Trial data must conform to the JSON schema located in `Schemas/clinicaltrial-schema.json`. Key validations include:
- Required fields: trialId, title, startDate, status
- Date format validation
- Enum validation for status
- Additional property checks

## License

This project is licensed under the MIT License - see the LICENSE file for details