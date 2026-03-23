# ServiceRegistry

A modern microservices playground built with ASP.NET Core, demonstrating service discovery, inter-service communication, and containerization patterns using Docker and Kubernetes.

**Tech Stack**: ASP.NET Core 8 | Docker | Kubernetes (AKS) | dapr | gRPC (planned)

---

## 📋 Table of Contents

- [Architecture](#architecture)
- [Features](#features)
- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Environment Variables](#environment-variables)
- [Development](#development)
- [Deployment](#deployment)
- [Future Enhancements](#future-enhancements)

---

## Architecture

### System Design

```
┌────────────────┐         ┌────────────────┐
│   Echo App 1   │         │   Echo App 2   │
│  (Port 5001)   │         │  (Port 5002)   │
└────────┬───────┘         └────────┬───────┘
         │                          │
         │     ┌─────────────────┐  │
         ├────►│ ServiceRegistry │◄─┤
         │     │  (Port 7089)    │  │
         │     └─────────────────┘  │
         │                          │
         └──────────────┬───────────┘
                        │
                   Discovers Services
                   Registers Location
                   Renews Registration
```

### Communication Flow

1. **Service Registration**: Echo services register with ServiceRegistry on startup
2. **Service Discovery**: Services query ServiceRegistry to locate other services
3. **Inter-service Calls**: Services invoke each other using discovered URLs
4. **Service Health**: Services renew registrations at regular intervals (30s)
5. **Graceful Deregistration**: On shutdown, services are automatically deregistered

---

## Features

### ServiceRegistry Service
- **RESTful API** for service registration and discovery
- **In-memory registry** with concurrent dictionary for thread-safe operations
- **Service timeout handling** - removes unresponsive services after 2 minutes
- **Round-robin load balancing** - returns next available instance for a service key
- **Comprehensive logging** - detailed service lifecycle tracking
- **XML documentation** - full API documentation via Swagger UI

### Echo Service
- **Simple echo endpoint** - `/Echo/{text}` returns text with configurable wrapping
- **Service forwarding** - can forward echo requests to other Echo instances
- **Auto-registration** - automatically registers with ServiceRegistry on startup
- **Health renewal** - periodic registration renewal every 30 seconds
- **Graceful shutdown** - automatic service deregistration on app shutdown
- **Error resilience** - handles connection failures gracefully

### Cross-Cutting Concerns
- **Structured logging** - source generators for compile-time safety
- **Dependency injection** - clean service architecture
- **Configuration via environment variables**
- **Containerization** - Docker support with optimized images
- **Kubernetes ready** - deployment manifests included

---

## Project Structure

```
ServiceRegistry/
├── src/
│   ├── ServiceRegistry/                 # Service Registry App
│   │   ├── Controllers/
│   │   │   └── ServiceController.cs    # REST API endpoints
│   │   ├── ServerRegistry.cs           # Core registry logic
│   │   ├── ServiceList.cs              # Queue-based service management
│   │   ├── LogMessages.cs              # Logging definitions
│   │   ├── Program.cs                  # App configuration
│   │   └── appsettings.*.json         # Configuration files
│   │
│   ├── Echo/                            # Echo Service App
│   │   ├── Program.cs                  # Service registration & endpoints
│   │   ├── appsettings.*.json         # Configuration files
│   │   └── Echo.http                   # HTTP test requests
│   │
│   ├── ServiceRegistry.Interface/       # Shared Models
│   │   ├── Service.cs                  # Service model
│   │   ├── ServiceKey.cs               # Service identifier
│   │   └── Location.cs                 # Service location
│   │
│   ├── Dockerfile-serviceregistry      # ServiceRegistry image
│   ├── Dockerfile-echo                 # Echo service image
│   ├── Kubernetes-serviceregistry.yaml # K8s deployment manifest
│   └── Kubernetes-echo.yaml            # K8s deployment manifest
│
├── test/                                 # Test projects
├── templates/                            # Azure templates
├── scripts/                              # Deployment scripts
│   ├── az-acr.ps1                      # Azure Container Registry setup
│   ├── az-appservice.ps1               # Azure App Service deployment
│   └── az-api-test.ps1                 # API testing script
│
└── README.md                             # This file
```

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker (for containerized deployment)
- Kubernetes cluster (for K8s deployment)

### Local Development

1. **Build projects**:
   ```powershell
   dotnet build
   ```

2. **Start ServiceRegistry** (Terminal 1):
   ```powershell
   dotnet run --project src/ServiceRegistry/ServiceRegistry.csproj
   ```
   - Accessible at: `https://localhost:7089`
   - Swagger UI: `https://localhost:7089/swagger`

3. **Start Echo service** (Terminal 2):
   ```powershell
   dotnet run --project src/Echo/Echo.csproj
   ```
   - Accessible at: `https://localhost:7001`

4. **Test the echo endpoint**:
   ```bash
   curl https://localhost:7001/Echo/Hello
   # Response: "Hello"
   ```

### Docker Deployment

1. **Build images**:
   ```powershell
   docker build -f src/Dockerfile-serviceregistry -t serviceregistry:v1 src
   docker build -f src/Dockerfile-echo -t echo:v1 src
   ```

2. **Run containers** (Docker Compose recommended):
   ```powershell
   docker-compose up
   ```

3. **Test via curl**:
   ```bash
   curl http://host.docker.internal:7089/Service/dev/Echo
   ```

---

## API Reference

### Endpoints

#### Get Service
```http
GET /Service/{environment}/{name}
```
- **Description**: Retrieve a registered service location
- **Parameters**:
  - `environment`: Service environment (e.g., "dev", "staging")
  - `name`: Service name (e.g., "Echo")
- **Response**: Service location URL
- **Example**: `GET /Service/dev/Echo` → `http://localhost:5001/Echo`

#### Register Service
```http
POST /Service
Content-Type: application/json

{
  "key": {
    "environment": "dev",
    "name": "Echo"
  },
  "location": {
    "scheme": "http",
    "host": "localhost:5001",
    "path": "/Echo"
  }
}
```

#### Renew Service Registration
```http
PUT /Service
Content-Type: application/json

{
  "key": { "environment": "dev", "name": "Echo" },
  "location": { "scheme": "http", "host": "localhost:5001", "path": "/Echo" }
}
```
- **Description**: Refresh heartbeat; required every 2 minutes to keep service active

#### Deregister Service
```http
DELETE /Service/{location}
```
- **Description**: Remove service registration
- **Parameter**: `location` - URL-encoded service location
- **Example**: `DELETE /Service/http%3A%2F%2Flocalhost%3A5001%2FEcho`

---

## Environment Variables

### ServiceRegistry
| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_URLS` | `https://localhost:7089` | Server endpoint |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Environment mode |

### Echo Service
| Variable | Required | Description |
|----------|----------|-------------|
| `SERVICE_REGISTRY_ENDPOINT` | Yes | ServiceRegistry API URL |
| `SERVICE_ENVIRONMENT` | Yes | Environment name (dev/staging/prod) |
| `SERVICE_LABEL` | Yes | Service instance name |
| `ASPNETCORE_URLS` | Yes | Echo service endpoint |
| `ECHO_TEXT` | No | Text wrapper (default: empty) |
| `ECHO_FORWARD` | No | Target Echo service for forwarding |

**Example Docker environment file**:
```env
SERVICE_REGISTRY_ENDPOINT=http://serviceregistry:7089/Service
SERVICE_ENVIRONMENT=dev
SERVICE_LABEL=echo-1
ASPNETCORE_URLS=http://0.0.0.0:5001
ECHO_TEXT="[Echo] "
ECHO_FORWARD=Echo
```

---

## Development

### Building

```powershell
# Full solution
dotnet build

# Specific project
dotnet build src/ServiceRegistry/ServiceRegistry.csproj
dotnet build src/Echo/Echo.csproj
```

### Testing

Use the included `.http` files in each project:
```powershell
# Visual Studio Code REST Client extension
# Open Echo.http and send requests
```

Or use PowerShell scripts:
```powershell
.\az-api-test.ps1
```

### Code Quality

- **Nullable reference types**: Enabled
- **Source-generated logging**: For compile-time safety
- **XML documentation**: All public members documented

---

## Deployment

### Azure Container Registry

```powershell
./az-acr.ps1 -ACR_NAME "<registry-name>" -RES_GROUP "<resource-group>"
```

### Azure App Service

```powershell
./az-appservice.ps1
```

### Kubernetes (AKS)

1. **Push images to registry**:
   ```powershell
   docker tag serviceregistry:v1 $ACR.azurecr.io/serviceregistry:v1
   docker push $ACR.azurecr.io/serviceregistry:v1
   ```

2. **Deploy manifests**:
   ```bash
   kubectl apply -f src/Kubernetes-serviceregistry.yaml
   kubectl apply -f src/Kubernetes-echo.yaml
   ```

3. **Check status**:
   ```bash
   kubectl get pods
   kubectl logs -f deployment/serviceregistry
   ```

---

## Future Enhancements

### Planned Features
- **dapr Integration**:
  - Publish and subscribe messaging
  - Service-to-service invocation improvements
  - State management for service metadata
  - Actor model for long-running operations

- **Advanced Service Discovery**:
  - Health check endpoints
  - Service mesh integration (Istio)
  - Weighted load balancing

- **gRPC Support**: High-performance inter-service communication

- **Tracing & Observability**:
  - Distributed tracing (OpenTelemetry)
  - Metrics collection (Prometheus)
  - Health dashboards

- **Security**:
  - Service-to-service authentication
  - TLS for inter-service communication
  - Secrets management integration

---

## License

See [LICENSE](LICENSE) file for details.

---

## Contributing

Contributions welcome! This is a playground project for learning and experimentation.