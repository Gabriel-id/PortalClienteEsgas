# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 customer portal solution for gas utility services (Portal Cliente Esgas) consisting of:
- **PortalClienteAPI**: Web API (.NET 8 Windows) for backend services with JWT authentication
- **PortalCliente**: MVC web application (.NET 8) for customer frontend with cookie authentication
- **PortalCliente.Core**: Core domain layer with services and interfaces
- **PortalCliente.Infrastructure**: Infrastructure layer with external service integrations

## Architecture

The solution follows a layered architecture:
- **API Layer** (PortalClienteAPI): Exposes REST endpoints, handles JWT authentication, integrates with SAP
- **Presentation Layer** (PortalCliente): MVC controllers and views, cookie-based authentication
- **Core Layer** (PortalCliente.Core): Business logic, DTOs, service interfaces
- **Infrastructure Layer** (PortalCliente.Infrastructure): External service implementations (SAP integration)

Key dependencies:
- API uses JWT Bearer authentication with hardcoded secret
- Frontend uses cookie authentication with 1-hour expiration
- SAP integration via HTTP client with basic auth credentials
- Frontend uses LibMan for client-side libraries (Font Awesome, Toastr)

## Development Commands

Build the entire solution:
```
dotnet build PortalCliente.sln
```

Build specific projects:
```
dotnet build PortalClienteAPI/PortalClienteAPI.csproj
dotnet build PortalCliente/PortalCliente.csproj
```

Run the API (development):
```
cd PortalClienteAPI
dotnet run
```
- API runs on https://localhost:7044 and http://localhost:5031

Run the web application:
```
cd PortalCliente
dotnet run
```

Publish for deployment:
```
dotnet publish -c Release
```

Restore packages:
```
dotnet restore
```

Update client-side libraries:
```
cd PortalCliente
libman restore
```

## Service Integration

The solution integrates with SAP via:
- Base URL: `http://srv-sap-prd.esgas.com.br:8000/sap/bc/inbound`
- Authentication: Basic auth with embedded credentials
- Used for invoice data retrieval and customer authentication

## Authentication Flow

1. Frontend (PortalCliente) authenticates users via cookie auth
2. API (PortalClienteAPI) uses JWT tokens for service-to-service communication
3. Both layers integrate with SAP for user validation and data retrieval

## Key Services

- **AuthService**: Handles user authentication with validation
- **InvoicesService**: Manages customer invoice operations
- **SapService**: Integrates with SAP system for data retrieval (uses IHttpClientFactory)
- **TokenService**: JWT token generation and validation

## Architecture Improvements Implemented

### Error Handling
- Global exception handling middleware for consistent error responses
- Structured logging with Serilog (console and file outputs)
- Proper HTTP status codes and error messages

### Validation
- FluentValidation for input validation on Login and AuthRequest DTOs
- Client-side and server-side validation integration
- Proper ModelState handling in controllers

### Performance & Best Practices
- IHttpClientFactory implementation for efficient HTTP connections
- DTOs converted from struct to class for better memory management
- Dependency injection properly implemented throughout
- Standardized English naming conventions

### Logging Configuration
- Logs written to `logs/portal-cliente-{date}.txt`
- Structured JSON logging with context enrichment
- Console and file sinks configured
- Different log levels for Microsoft/System vs application code