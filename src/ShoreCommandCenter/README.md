# Shore Command Center

The Shore Command Center is a comprehensive fleet management system for monitoring and deploying updates to cruise ship containers.

## Features

- **Fleet Management**: Monitor all ships in the fleet with real-time status updates
- **Container Deployment**: Deploy container updates to individual ships or the entire fleet
- **Health Monitoring**: Track system health metrics for each ship
- **Real-time Dashboard**: SignalR-powered real-time updates
- **Background Jobs**: Automated tasks using Hangfire
- **API Security**: JWT-based authentication
- **Database Integration**: Entity Framework Core with SQL Server

## API Endpoints

### Ship Management
- `POST /api/ships/register` - Register a new ship
- `GET /api/ships` - Get all ships
- `GET /api/ships/{shipId}` - Get specific ship details
- `PUT /api/ships/{shipId}/status` - Update ship status
- `POST /api/ships/{shipId}/health` - Submit health metrics

### Fleet Operations
- `GET /api/fleet/status` - Get fleet overview
- `POST /api/fleet/deploy` - Deploy to entire fleet
- `GET /api/fleet/metrics` - Get fleet health summary
- `GET /api/fleet/deployments` - Get recent fleet deployments

### Dashboard
- `GET /api/dashboard/overview` - Get dashboard overview
- `GET /api/dashboard/alerts` - Get active alerts
- `GET /api/dashboard/metrics` - Get real-time metrics

## Configuration

### Database Connection
Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShoreCommandCenter;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### JWT Authentication
Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-minimum-256-bits",
    "Issuer": "ShoreCommandCenter",
    "Audience": "CruiseShipFleet",
    "ExpiryInHours": 24
  }
}
```

### Container Registry
Configure container registry settings:

```json
{
  "ContainerRegistry": {
    "Url": "your-registry-url",
    "Type": "Docker",
    "IsSecure": true
  }
}
```

## Running the Application

### Development
```bash
dotnet run
```

### Docker
```bash
docker build -t shore-command-center .
docker run -p 8080:8080 shore-command-center
```

## Database Migrations

Create and apply migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Real-time Features

The application includes SignalR hubs for real-time communication:

- **FleetMonitoringHub**: Real-time fleet status updates
- Automatic notifications for ship status changes
- Live deployment progress updates
- Health metrics streaming

## Background Jobs

Hangfire is configured for background processing:

- Health metrics cleanup
- Expired update request cleanup
- Fleet status monitoring
- Alert processing

## Security

- JWT-based API authentication
- CORS configuration for web clients
- Input validation and sanitization
- Secure headers configuration

## Logging

Structured logging with different levels:
- Information: General application flow
- Warning: Unexpected but non-critical issues
- Error: Error events but application continues
- Critical: Critical errors that might cause application to abort

## Monitoring

The system provides comprehensive monitoring capabilities:
- Ship health metrics (CPU, Memory, Disk, Network)
- Deployment status tracking
- Fleet-wide statistics
- Alert system for critical events
