# MicroservicesSolution


# Architecture
# API Gateway (Ocelot)
Single entry point for all requests. Routes traffic to services and validates JWT tokens. I used Ocelot because it's simple to configure and handles routing well.
# Microservices (Clean Architecture)
Each service follows a layered architecture:

Domain: Business entities (User, Order)
Application: Business logic, DTOs, and service interfaces
Infrastructure: EF Core repositories, database context, migrations
API: REST controllers, authentication setup

# Shared Components

Auth: Keycloak JWT validation logic
Logging: Serilog for structured logs
Middleware: Exception handling and correlation IDs

Authentication
Keycloak handles Each service validates tokens independently. I configured realms, clients, and role-based authorization (admin, user).
Database
SQL Server with database-per-service pattern. Migrations needs to run for seeding data.
