# SylviaNG Prescription Microservice

## Overview

The Prescription microservice is part of the SylviaNG ecosystem, implementing **PrescriptionMS** — a hospital
prescription/EMR system for Admin, Doctor, and Staff roles (see `Doc/feature.md` and `Doc/user_stories.md` for
the full target spec). Authentication is real Keycloak-issued JWTs (see `Doc/auth-jwt.md`); the rest of the
domain (Patients, Consultations, Prescriptions, Medicines, Templates, ...) is built feature-by-feature on top of
the Clean Architecture/CQRS foundation described below.

## Technology Stack

- .NET 10.0
- Entity Framework Core 10.0
- PostgreSQL / SQL Server / Oracle (configurable)
- Keycloak Authentication (JWT)
- Finbuckle.MultiTenant for multi-tenancy support
- MediatR for CQRS pattern
- FluentValidation for input validation
- Manual mapper extension methods for object mapping (no AutoMapper)
- gRPC for inter-service communication

## Project Structure

```
SylviaNG.Prescription/
├── Application/                        # Application layer (business logic, CQRS handlers)
│   ├── Common/
│   │   ├── Exceptions/                # Custom exceptions (NotFoundException, DuplicateException)
│   │   └── Models/                    # Shared DTOs (CoreGrpcModels)
│   ├── Extensions/
│   │   ├── AuthenticationExtensions.cs  # Keycloak JWT authentication setup
│   │   ├── AuthorizationExtensions.cs   # Authorization policy configuration
│   │   ├── DependencyInjection.cs       # Application service registrations
│   │   └── ValidationBehavior.cs        # MediatR pipeline validation behavior
│   ├── Features/
│   │   └── Auth/                      # Feature module (follow this pattern for new features)
│   │       ├── Commands/              # CQRS Commands (Login, Logout, RefreshToken, ... + Handlers + Validators)
│   │       └── Models/                # DTOs (Request/Response models)
│   ├── Interfaces/
│   │   ├── Externals/                 # External service interfaces (ICoreGrpcClient, IKeycloakTokenClient, IKeycloakAdminClient)
│   │   ├── Repositories/              # Repository interfaces (IUserRepository)
│   │   └── Services/                  # Service interfaces (IAuthService)
│   ├── Mappings/                      # Manual mapper extension methods (no AutoMapper)
│   └── Services/                      # Business logic service implementations
├── Domain/                            # Domain layer (entities, value objects, domain events)
│   ├── Entities/                      # Business entities (User; more land feature-by-feature)
│   ├── Enums/                         # Domain enumerations
│   ├── Events/                        # Domain events
│   └── ValueObjects/                  # Value objects (if needed)
├── Infrastructure/                    # Infrastructure layer (data access, external services)
│   ├── Configurations/                # EF Core entity configurations
│   ├── Data/
│   │   └── ApplicationDBContext.cs   # DbContext with multi-tenancy
│   ├── Extensions/
│   │   ├── DependencyInjection.cs     # Infrastructure service registrations
│   │   ├── GrpcExtensions.cs          # gRPC client registration
│   │   └── KeycloakExtensions.cs      # Keycloak admin/token client registration
│   ├── Interceptors/                  # EF Core interceptors (Audit, UtcDateTime)
│   ├── MultiTenancy/                  # Tenant info model
│   ├── Repositories/                  # Repository implementations
│   └── Services/                      # External service implementations (CoreGrpcClient, KeycloakAdminClient, KeycloakTokenClient)
├── Controllers/                       # API controllers
│   └── AuthController.cs              # Login/refresh/logout/password-reset endpoints
├── Middlewares/                       # Custom middleware components
│   ├── GlobalExceptionHandlerMiddleware.cs
│   └── ResponseWrappingMiddleware.cs
├── SharedKernel/                      # Shared components
│   ├── Audit/                         # Base audit entity
│   ├── Generic/                       # Generic repository + unit of work
│   ├── Pagination/                    # Pagination support
│   └── Utils/                         # DateTime utilities, JSON converters
├── Protos/                            # gRPC proto definitions
│   └── core.proto
├── Migrations/                        # EF Core migrations
├── Program.cs                         # Application entry point
├── appsettings.json                   # Configuration
└── Dockerfile                         # Docker support

SylviaNG.Prescription.Tests/            # Unit and integration tests
├── Controllers/                       # Controller tests
├── Services/                          # Service tests
└── Validators/                        # Validator tests
```

## Architecture Pattern

This project follows **Clean Architecture** with **Domain-Driven Design (DDD)** and **CQRS**:

```
┌──────────────────────────────────────────────────┐
│                   Controllers                     │  ← API endpoints
├──────────────────────────────────────────────────┤
│                  Application                      │  ← Business logic, CQRS, Services
│           (MediatR Handlers, Validators)          │
├──────────────────────────────────────────────────┤
│                    Domain                         │  ← Entities, Events, Enums
├──────────────────────────────────────────────────┤
│                Infrastructure                     │  ← Data access, Keycloak, gRPC
│         (EF Core, Repositories, Interceptors)     │
├──────────────────────────────────────────────────┤
│                SharedKernel                       │  ← Generic repo, Audit, Pagination
└──────────────────────────────────────────────────┘
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL / SQL Server / Oracle database
- Keycloak instance for authentication

### Configuration

For local development, bring up Postgres + Keycloak via Docker Compose (see `SylviaNG.Prescription/Doc/local-dev-infrastructure.md` for full setup/verification steps), then copy `.env.example` → `.env` and `SylviaNG.Prescription/appsettings.Development.json.example` → `appsettings.Development.json`:

```json
{
  "Database": {
    "Provider": "Postgresql",
    "ConnectionString": "Host=localhost;Port=5432;Database=prescriptionms;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/prescriptionms",
    "ClientId": "prescriptionms-backend",
    "ClientSecret": "dev-only-secret-change-me"
  }
}
```

### Running the Service

```bash
cd SylviaNG.Prescription
dotnet restore
dotnet run
```

The service will start on:

- HTTP: http://localhost:5208
- HTTPS: https://localhost:7208

### API Documentation

Once running, access Swagger UI at: `http://localhost:5208/swagger`

## Features

- **Multi-tenant support** via JWT claims (`tenant_id`)
- **Clean Architecture** with strict layer separation
- **CQRS with MediatR** — Commands and Queries separated
- **Repository Pattern** with generic base + Unit of Work
- **Global exception handling** with consistent error response format
- **Response wrapping middleware** — All responses wrapped in `{ hasError, decentMessage, errorDetails, content }`
- **Audit logging** — All entities inherit from `Audit` base class
- **UTC DateTime enforcement** via EF Core interceptor
- **gRPC** for inter-service communication with Core microservice
- **FluentValidation** integrated into MediatR pipeline

## API Response Format

All API responses follow this standard envelope:

```json
{
  "hasError": false,
  "decentMessage": "Request processed successfully.",
  "errorDetails": null,
  "content": { }
}
```

Error responses:

```json
{
  "hasError": true,
  "decentMessage": "Validation failed.",
  "errorDetails": ["Title is required."],
  "content": null
}
```

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## Testing

```bash
cd SylviaNG.Prescription.Tests
dotnet test
```

## Docker Support

Build and run using Docker:

```bash
docker build -t sylviang-prescription .
docker run -p 5208:5002 sylviang-prescription
```

## How to Add a New Feature

Follow the existing `Auth` feature's structure (see `Doc/auth-jwt.md` for what/why):

1. **Domain** — Create entity in `Domain/Entities/` inheriting from `Audit`
2. **Infrastructure** — Add `DbSet` in `ApplicationDBContext`, create configuration in `Configurations/`, create repository in `Repositories/`
3. **Application** — Create feature folder in `Features/` with `Commands/` (and `Queries/` for a typical CRUD entity), `Models/` subfolders
4. **Mappings** — Add a manual mapper extension method in `Mappings/` (no AutoMapper in this project)
5. **Services** — Create service interface in `Interfaces/Services/` and implementation in `Services/`
6. **DI** — Register repository in `Infrastructure/Extensions/DependencyInjection.cs` and service in `Application/Extensions/DependencyInjection.cs`
7. **Controller** — Create controller in `Controllers/` using MediatR for CQRS
8. **Tests** — Add service, controller, and validator tests in `Tests/`
9. **Docs** — Write `Doc/<feature-name>.md`: what the feature is, what it does, why it was built

## Related Projects

- **SylviaNG.Cafeteria** — Cafeteria management microservice (reference architecture)
- **SylviaNG.LMS** — Learning Management System microservice
