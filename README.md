# Articles API

A portfolio project demonstrating a **microservices-based** article platform built with **ASP.NET Core (.NET 10)**. The system allows users to register, authenticate, publish articles, leave comments, and receive email notifications — all through independently deployable services communicating via an asynchronous message bus.

> **Note:** An AI coding agent was used to assist with parts of the implementation, but the overall architecture, design decisions, and concepts are entirely my own.

---

## Architecture Overview

The solution follows a **microservices architecture** with five projects:

```
                              ┌──────────────────────┐
              ┌──────────────►│  AuthenticationService│──┐
              │  /api/v1/auth │       (Web API)       │  │
              │               └──────────────────────┘  │
┌───────────────────┐                                    │  Azure Service Bus   ┌──────────────────────────┐
│    ApiGateway     │                                    ├────(article-queue)──►│   NotificationService    │
│   (YARP Proxy)    │                                    │                      │   (.NET Worker Service)  │
└───────────────────┘                                    │                      └──────────────────────────┘
              │               ┌──────────────────────┐  │
              └──────────────►│   ArticleService     │──┘
               /api/v1/       │       (Web API)      │
               articles       └──────────────────────┘

                              ┌──────────────────────┐
                              │       Events         │  (Shared library — domain event contracts)
                              └──────────────────────┘
```

| Service | Type | Responsibility |
|---|---|---|
| **ApiGateway** | ASP.NET Core + YARP | Reverse proxy / API gateway — single entry point that routes requests to backend services and aggregates OpenAPI documentation |
| **AuthenticationService** | ASP.NET Core Web API | User registration, login, JWT token issuance & refresh, token revocation, forgot/reset password |
| **ArticleService** | ASP.NET Core Web API | CRUD operations for articles and comments, authorization enforcement |
| **NotificationService** | .NET Worker Service | Consumes domain events and sends transactional emails (welcome, comment alerts, password resets) |
| **Events** | Class Library | Shared domain event contracts used across services |

---

## Key Skills & Technologies Demonstrated

### Microservices & Distributed Systems
- Independent, single-responsibility services with separate databases and deployment units
- **API Gateway** using **YARP (Yet Another Reverse Proxy)** as a single entry point with route-based request forwarding
- Aggregated **Swagger UI** at the gateway proxying OpenAPI documents from downstream services
- **Asynchronous messaging** via **Azure Service Bus** for decoupled inter-service communication
- **Wolverine** as the message bus / mediator framework with durable outbox messaging and Azure Service Bus transport
- Shared **domain event contracts** (`UserCreatedEvent`, `CommentAddedEvent`, `CommentDeletedEvent`, `PasswordResetRequestedEvent`)

### Authentication & Security
- **ASP.NET Core Identity** for user management with configurable password policies and account lockout
- **JWT Bearer authentication** with access & refresh token flow
- Token revocation support
- Forgot password / reset password flow with email verification
- Claims-based authorization (owner-only article editing/deletion)

### Data Access & Persistence
- **Entity Framework Core** with **PostgreSQL** (`Npgsql` provider)
- Code-First **migrations** with automatic migration on startup in production
- **Repository pattern** with `IArticleRepository` abstraction
- **Unit of Work pattern** for transactional consistency
- Wolverine durable message persistence in PostgreSQL

### API Design & Documentation
- RESTful API design with proper HTTP status codes and `ProblemDetails` error responses
- **API versioning** (`/api/v1/...`) via `Asp.Versioning`
- **OpenAPI / Swagger** documentation with XML doc comments
- JWT-secured Swagger UI for interactive testing

### Cloud & Azure Integration
- **Azure App Configuration** for centralized, environment-aware configuration with label filters and key prefix trimming
- **Azure Key Vault** integration for secret management (referenced via App Configuration)
- **Azure Service Bus** as the production message broker
- Dynamic configuration refresh with sentinel key monitoring (30-second polling)
- **Azure App Configuration refresh** implemented as a `BackgroundService` in the Worker Service

### Containerization & DevOps
- **Dockerfiles** for each service (multi-stage builds for optimized images)
- **Docker Compose** orchestration for local development
- Container-ready configuration with environment variable support

### Email & Notifications
- **MailKit / MimeKit** for SMTP email delivery with TLS support
- Options pattern with **data annotation validation** (`ValidateOnStart`) for email settings
- Event-driven notification handlers for:
  - Welcome email on user registration
  - Comment notification to article authors
  - Password reset link delivery

### Error Handling & Resilience
- Global exception handling via `IExceptionHandler` with structured `ProblemDetails` responses
- Azure Service Bus **exponential retry** policies
- Wolverine **durable local queues** and EF Core transactional outbox for reliable message delivery
- Health checks for PostgreSQL, Azure Service Bus, and EF Core database connectivity

### Code Quality & Patterns
- **Dependency Injection** throughout, organized via extension method registrations (`DependencyInjection.cs`)
- Clean separation of concerns: Controllers → Services → Repositories
- Interface-based abstractions for testability
- C# modern features: primary constructors, records, collection expressions, raw string literals
- XML documentation on public APIs

---

## Project Structure

```
articles-api/
├── ApiGateway/                 # YARP reverse proxy / API gateway
│   ├── Dockerfile
│   └── Program.cs
├── ArticleService/             # Articles & comments Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── DbContexts/
│   ├── Migrations/
│   ├── Dockerfile
│   └── Program.cs
├── AuthenticationService/      # Auth & identity Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── DbContexts/
│   ├── Migrations/
│   ├── Dockerfile
│   └── Program.cs
├── NotificationService/        # Event-driven email worker
│   ├── Handlers/
│   ├── Services/
│   ├── Settings/
│   ├── Dockerfile
│   └── Program.cs
├── Events/                     # Shared domain event contracts
├── docker-compose.yml
└── README.md
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (for containerized runs)
- PostgreSQL instance (or use Docker)
- Azure Service Bus namespace (or local emulator)
- Azure App Configuration store with the required keys
- SMTP server for email delivery

### Running with Docker Compose

```bash
docker-compose up --build
```

### Running Locally

1. Configure connection strings in each service's `appsettings.json` or User Secrets:
   - `AzureAppConfiguration` — connection string to Azure App Configuration
   - `DefaultConnection` — PostgreSQL connection string
   - `AzureServiceBus` — Azure Service Bus connection string
2. Run each service:
   ```bash
   dotnet run --project ApiGateway
   dotnet run --project ArticleService
   dotnet run --project AuthenticationService
   dotnet run --project NotificationService
   ```

---

## API Endpoints

All requests are routed through the **ApiGateway** (default `http://localhost:5000` in development).

### AuthenticationService (`/api/v1/auth`)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/register` | Register a new user |
| POST | `/login` | Login and receive JWT tokens |
| POST | `/refresh-token` | Refresh an expired access token |
| POST | `/revoke-token` | Revoke a refresh token |
| POST | `/forgot-password` | Request a password reset email |
| POST | `/reset-password` | Reset password with a valid token |

### ArticleService (`/api/v1/articles`)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List all articles |
| GET | `/{id}` | Get article by ID |
| POST | `/` | Create a new article *(auth required)* |
| PUT | `/{id}` | Update an article *(owner only)* |
| DELETE | `/{id}` | Delete an article *(owner only)* |
| GET | `/{id}/comments` | List comments on an article |
| POST | `/{id}/comments` | Add a comment *(auth required)* |
| DELETE | `/{id}/comments/{commentId}` | Delete a comment *(owner only)* |
