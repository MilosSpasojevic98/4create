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

```cmd
git clone <repository-url>
cd ClinicalTrialApp
```

### 2. Run with Docker Compose (Recommended)

```cmd
REM Build and start the containers
docker compose up --build

REM Stop the containers
docker compose down
```

The API will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

Swagger UI will be available at: https://localhost:5001

### 3. Development Certificates

If you get certificate errors:

```cmd
dotnet dev-certs https --clean
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p your_password
dotnet dev-certs https --trust
```

Update the password in docker-compose.yml:
```yaml
environment:
  - ASPNETCORE_Kestrel__Certificates__Default__Password=your_password
```

## API Endpoints

### Upload Trial Data
```http
POST /api/trials/upload
Content-Type: multipart/form-data

file=@trial.json
```

Response:
```json
{
    "message": "Trial data processed successfully",
    "trialId": "123e4567-e89b-12d3-a456-426614174000"
}
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

## Business Rules

- Trial dates are automatically converted to UTC
- Duration is calculated for trials with end dates
- Ongoing trials without end dates get a default 1-month duration
- Duplicate trialIds are not allowed
- File size limit of 1MB
- Only JSON files are accepted

## Schema Validation

Trial data must conform to the JSON schema located in `Schemas/clinicaltrial-schema.json`. Key validations include:
- Required fields: trialId, title, startDate, status
- Date format validation
- Enum validation for status (Ongoing, Completed, Terminated)
- Additional property checks

## Running Tests

```cmd
dotnet test
```

The test suite includes:
- Unit tests for core business logic
- Integration tests using test containers
- Validation tests for JSON schema
- Command handler tests
- Service layer tests

## Error Handling

The API provides structured error responses:
```json
{
    "status": "Error",
    "message": "Descriptive error message"
}
```

Common error scenarios:
- Invalid JSON format
- Schema validation failures
- Duplicate trial IDs
- File size exceeded
- Invalid file type

## Architecture

The application follows clean architecture principles:
- CQRS with MediatR
- Repository pattern
- Result pattern for error handling
- Event-driven design
- Fluent Validation
- Middleware-based error handling

## Environment Variables

Key environment variables (set in docker-compose.yml):
```yaml
- ASPNETCORE_ENVIRONMENT=Development
- ASPNETCORE_URLS=https://+:5001;http://+:5000
- ConnectionStrings__DefaultConnection=Host=db;Database=clinicalTrial;Username=postgres;Password=admin
```

## License

This project is licensed under the MIT License - see the LICENSE file for details


