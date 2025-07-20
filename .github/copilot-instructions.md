# Copilot Instructions for Employee Management System

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a Blazor Server application for Employee Management following Clean Architecture principles, SOLID principles, and best practices. The project uses DevExpress components for rich UI controls.

## Architecture
- **Domain Layer**: Contains entities, value objects, domain services, and business rules
- **Application Layer**: Contains use cases, interfaces, DTOs, and application services
- **Infrastructure Layer**: Contains data access, external services, and infrastructure concerns
- **Presentation Layer (Web)**: Blazor Server application with DevExpress components

## Development Guidelines
- Follow Clean Architecture principles
- Implement SOLID principles
- Use dependency injection
- Implement proper error handling and logging
- Use async/await patterns for database operations
- Follow naming conventions and code style guidelines
- Write unit tests for business logic
- Use DevExpress components for UI controls

## Technology Stack
- .NET 9.0
- Blazor Server
- DevExpress Blazor Components
- Entity Framework Core (for future database integration)
- Azure services for deployment

## Code Style
- Use C# naming conventions
- Prefer explicit types over var when type is not obvious
- Use meaningful variable and method names
- Keep methods small and focused
- Use regions to organize code sections
