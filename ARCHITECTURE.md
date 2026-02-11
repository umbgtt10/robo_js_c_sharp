# Robotic Control System - Architecture

## Overview

This is a production-grade web-based robotic control system built with React (frontend), ASP.NET Core (backend), and TCP-based hardware communication. The system enables real-time monitoring and control of industrial robotic hardware through a modern web interface.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend (React SPA)                     │
│                      Port 5173                              │
├─────────────────────────────────────────────────────────────┤
│  • React 18 + TypeScript + Vite                             │
│  • SignalR Client (WebSocket real-time updates)             │
│  • Axios (REST API calls)                                    │
│  • TailwindCSS (Styling)                                     │
└────────┬──────────────────────────────────────────────┬─────┘
         │ HTTP/HTTPS (REST)                     │ WebSocket
         │ User Commands                         │ Real-time Updates
         ▼                                       ▼
┌──────────────────────────────────────────────────────────────┐
│              Backend API (.NET 9)                            │
│                  Port 5001                                   │
├──────────────────────────────────────────────────────────────┤
│  • Clean Architecture (Domain/Application/Infrastructure)   │
│  • SignalR Hub (Real-time broadcasting)                      │
│  • JWT Authentication + Authorization                        │
│  • Rate Limiting + FluentValidation                          │
│  • AutoMapper + Serilog                                      │
└───────────────────────────────┬──────────────────────────────┘
                                │ TCP/IP (Text Protocol)
                                ▼
┌──────────────────────────────────────────────────────────────┐
│           Robot Hardware / Simulator                         │
│                   Port 5000                                  │
├──────────────────────────────────────────────────────────────┤
│  • TCP Server                                                │
│  • Command Parser (MOVE, GET_POS, STOP, etc.)               │
│  • Position/Status Simulation                                │
└──────────────────────────────────────────────────────────────┘
```

## Clean Architecture Layers

### Domain Layer (`RoboticControl.Domain`)
**Core business logic - no external dependencies**

**Entities:**
- `RobotPosition` - X, Y, Z coordinates, rotations, timestamp
- `RobotStatus` - Connection state, temperature, load, errors

**Enums:**
- `RobotState` - Idle, Moving, Homing, EmergencyStopped, Error, Disconnected
- `ErrorCode` - Hardware error codes
- `CommandType` - Move, Jog, Stop, Home, ResetError

**Interfaces:**
- `IRobotHardwareService` - Hardware abstraction contract

**Exceptions:**
- `HardwareConnectionException` - Hardware communication failures
- `CommandValidationException` - Invalid command parameters

**Responsibilities:**
- Define business entities and value objects
- Encapsulate business rules
- Provide domain abstractions
- Framework-agnostic

---

### Application Layer (`RoboticControl.Application`)
**Business logic orchestration**

**Services:**
- `RobotControlService` - Core business logic
  - Coordinates hardware operations
  - Implements business rules (work envelope validation)
  - Manages command execution flow

**DTOs (Data Transfer Objects):**
- `RobotPositionDto`, `RobotStatusDto`, `MoveCommandDto`, `JogCommandDto`

**Validators (FluentValidation):**
- `MoveCommandValidator` - Range checks, work envelope validation
- `JogCommandValidator` - Single-axis check, safe limits

**Mappings:**
- `MappingProfile` - AutoMapper configuration (Entity ↔ DTO)

**Responsibilities:**
- Business rule enforcement
- Use case implementation
- Data transformation and validation
- Application workflow coordination

---

### Infrastructure Layer (`RoboticControl.Infrastructure`)
**External system integration**

**Hardware Communication:**
- `TcpRobotClient` - Low-level TCP/IP socket communication
  - Connection management with timeouts
  - Protocol implementation
  - Command/response handling

- `RobotHardwareService` - Hardware abstraction service
  - Connection lifecycle with automatic reconnection
  - Exponential backoff (1s → 2s → 4s → 8s → 16s max)
  - Event publishing (PositionChanged, StatusChanged)
  - Thread-safe command execution (SemaphoreSlim)

**Configuration:**
- `HardwareSettings` - Configuration model
  - Host/Port, Timeouts, Reconnection policies

**Responsibilities:**
- External system integration (TCP/IP)
- Connection resilience and retry logic
- Configuration management
- Protocol translation

---

### API Layer (`RoboticControl.API`)
**HTTP/WebSocket entry point**

**Controllers:**
- `RobotController` - Robot operations
- `AuthController` - JWT authentication
- `ConfigurationController` - System configuration

**SignalR Hubs:**
- `RobotHub` - Real-time position/status broadcasting

**Background Services:**
- `HardwareConnectionService` - Establishes hardware connection on startup
- `HardwareEventBroadcastService` - Subscribes to hardware events, broadcasts via SignalR

**Middleware:**
- `GlobalExceptionHandler` - Centralized error handling
- Rate Limiting - Per-endpoint policies
- JWT Authentication/Authorization

**Extensions:**
- `AuthenticationExtensions` - JWT configuration
- `RateLimitingExtensions` - Rate limit policies
- `ServiceCollectionExtensions` - DI registration
- `WebApplicationExtensions` - Middleware pipeline

**Responsibilities:**
- HTTP request handling and routing
- Input validation (FluentValidation)
- Real-time event broadcasting (SignalR)
- Background task orchestration

---

## Key Architectural Patterns

### 1. Clean Architecture / Onion Architecture

**Dependency Flow:**
```
API → Application → Domain
Infrastructure → Domain
```

**Benefits:**
- Domain logic independent of frameworks
- Easy to test (mock infrastructure)
- Flexible to swap implementations
- Clear separation of concerns

---

### 2. Repository Pattern

**Interface:** `IRobotHardwareService` (Domain)
**Implementation:** `RobotHardwareService` (Infrastructure)

**Benefits:**
- Abstraction over hardware communication
- Easy to swap real hardware for simulator
- Testability (mock hardware service)

---

### 3. Dependency Injection

All services registered in `Program.cs` via extension methods:

```csharp
// Singleton - shared instance
builder.Services.AddSingleton<TcpRobotClient>();
builder.Services.AddSingleton<IRobotHardwareService, RobotHardwareService>();

// Scoped - instance per HTTP request
builder.Services.AddScoped<RobotControlService>();

// Hosted Services - background tasks
builder.Services.AddHostedService<HardwareConnectionService>();
builder.Services.AddHostedService<HardwareEventBroadcastService>();
```

**Benefits:**
- Loose coupling
- Easy unit testing (inject mocks)
- Lifetime management by framework

---

### 4. CQRS-like Separation

**Commands (write operations):**
- `MoveToPosition`, `MoveRelative`, `EmergencyStop`, `Home`, `ResetError`

**Queries (read operations):**
- `GetPosition`, `GetStatus`

**Benefits:**
- Clear separation of read/write
- Different validation rules
- Optimized data access patterns

---

### 5. Event-Driven Communication

**Events:**
- `PositionChanged` - Fired when robot position updates
- `StatusChanged` - Fired when robot status changes

**Event Flow:**
```
Hardware Service → Event → EventBroadcastService → SignalR Hub → All Clients
```

**Benefits:**
- Decoupled components
- Real-time UI updates without polling
- Scalable to multiple subscribers

---

### 6. Background Services

**Services:**
- `HardwareConnectionService` - Initial connection on startup
- `HardwareEventBroadcastService` - Event subscription and SignalR broadcasting

**Benefits:**
- Long-running tasks don't block HTTP requests
- Automatic lifecycle management
- Graceful shutdown handling

---

## Data Flow Examples

### Example 1: Jog Command Flow

```
1. User clicks "+X" button in JogControls
2. Frontend: apiClient.jog({ deltaX: 10, deltaY: 0, deltaZ: 0 })
3. API: POST /api/robot/jog
4. RobotController receives request
5. FluentValidation validates (JogCommandValidator)
   - Single-axis check
   - Range validation (-100 to 100)
6. RobotControlService.JogAsync()
   - Get current position
   - Calculate resulting position
   - Validate work envelope
7. RobotHardwareService.MoveRelativeAsync()
   - Acquire command lock
8. TcpRobotClient.SendCommandAsync("MOVE_REL 10,0,0\n")
9. Simulator processes command, returns "OK"
10. Hardware service fires PositionChanged event
11. EventBroadcastService maps to DTO
12. SignalR broadcasts "PositionUpdate" to all clients
13. Frontend receives WebSocket message
14. React updates state via setPosition()
15. UI re-renders with new position
```

---

### Example 2: Real-Time Position Updates

```
1. EventBroadcastService subscribes to PositionChanged on startup
2. User executes command OR GET /api/robot/position called
3. Hardware service sends GET_POS via TCP
4. Receives position data from hardware
5. Fires PositionChanged event
6. EventBroadcastService handler executes
7. Maps RobotPosition → RobotPositionDto
8. Broadcasts: hub.Clients.All.SendAsync("PositionUpdate", dto)
9. All connected browsers receive WebSocket message
10. SignalR client invokes registered handler
11. React updates state
12. PositionDisplay component re-renders
```

---

### Example 3: Emergency Stop Flow

```
1. User clicks "Emergency Stop" button
2. Frontend → POST /api/robot/emergency-stop
3. RobotController.EmergencyStop() - no validation needed
4. RobotControlService.EmergencyStop() - bypass normal queue
5. RobotHardwareService.EmergencyStopAsync() - skip command lock (priority)
6. TcpRobotClient.SendCommandAsync("STOP\n")
7. Simulator halts motion, sets state to EmergencyStopped
8. Returns "OK"
9. StatusChanged event fires
10. SignalR broadcasts StatusUpdate
11. UI updates: red status, buttons disabled, only "Reset" enabled
```

---

## Cross-Cutting Concerns

### Logging (Serilog)

**Configuration:**
- Console sink (development debugging)
- File sink (rotating logs: `logs/robotcontrol-YYYYMMDD.txt`)
- Structured format (JSON-like)

**Log Levels:**
- `Debug` - Command/response details
- `Information` - Connections, movements
- `Warning` - Connection failures, retries
- `Error` - Unhandled exceptions

---

### Configuration Management

**Files:**
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

**Environment Variables (Production):**
- `JWT_SECRET_KEY` - Required, min 32 chars
- `HARDWARE__HOST` - Optional hardware IP override
- `HARDWARE__PORT` - Optional port override
- `CORS__ALLOWEDORIGINS__0` - Frontend URL

**Priority (lowest to highest):**
1. appsettings.json
2. appsettings.{Environment}.json
3. Environment variables
4. Command-line arguments

---

### Error Handling

**Strategy:** Multi-layer error handling

**Domain Layer:**
- Custom exceptions (`HardwareConnectionException`, `CommandValidationException`)

**Application Layer:**
- Catch infrastructure exceptions
- Transform to application-level errors

**API Layer:**
- `GlobalExceptionHandler` - Centralized exception handling
- Maps exceptions to HTTP status codes:
  - 400 Bad Request - Validation errors
  - 403 Forbidden - Authorization failures
  - 429 Too Many Requests - Rate limit exceeded
  - 500 Internal Server Error - Unexpected errors
  - 503 Service Unavailable - Hardware disconnected

**Infrastructure Layer:**
- Retry logic with exponential backoff
- Connection timeout handling
- Graceful degradation

---

### Security

**Authentication:**
- JWT Bearer tokens
- BCrypt password hashing (work factor 11)
- 8-hour token expiration (production), 24-hour (development)
- Environment variable for production secret

**Authorization:**
- Role-based policies:
  - `CanOperate` - Admin + Operator roles
  - `AdminOnly` - Admin role only
- Controller attribute: `[Authorize(Policy = "CanOperate")]`

**Rate Limiting:**
- Global: 200 req/min per IP (Token Bucket)
- Auth: 5 attempts/min per IP (brute force protection)
- Commands: 60/min per user (hardware protection)
- Queries: 100/min per user (monitoring)
- Config: 10/min per user

**CORS:**
- Whitelist specific origins
- Allow credentials (required for SignalR)
- Environment-specific origins

**Input Validation:**
- Client-side (TypeScript types)
- Server-side (FluentValidation)
- Domain-level (entity invariants)

---

### Resilience

**Connection Management:**
- Automatic reconnection with exponential backoff
- Maximum delay cap (16 seconds)
- Infinite retry attempts (configurable)
- Connection health monitoring

**Command Execution:**
- Thread-safe queueing (SemaphoreSlim)
- Timeout handling (5 seconds default)
- Emergency stop bypasses normal queue

**State Management:**
- Disconnected state handling
- Error state recovery (RESET command)
- Graceful degradation (UI shows status)

---

## Technology Decisions & Tradeoffs

### React over Angular/Vue
✅ Largest ecosystem for hardware visualization
✅ Better performance for real-time updates
✅ More component libraries for industrial UIs
❌ More boilerplate than Vue
❌ Requires additional libraries for routing/forms

### SignalR over Raw WebSockets
✅ Automatic reconnection
✅ Multiple transport fallbacks
✅ Type-safe client generation
❌ Larger bundle size
❌ Microsoft-specific

### TCP/IP over Serial
✅ Remote hardware access
✅ Easier testing with simulator
✅ Network-based deployment
❌ More network latency
❌ Requires IP configuration

### Clean Architecture
✅ Testability (mock infrastructure)
✅ Hardware abstraction swapping
✅ Industry best practices
❌ More initial setup
❌ More files/folders

### .NET 9 over Node.js Backend
✅ Better performance for CPU-bound tasks
✅ Strong typing with C#
✅ Mature enterprise patterns
✅ Better hardware communication libraries
❌ Requires .NET runtime
❌ Less flexibility than JavaScript

---

## Deployment Architecture

### Development (Current)
```
localhost:5173 (Frontend - Vite Dev Server)
    ↓ HTTPS REST + WebSocket
localhost:5001 (Backend - Kestrel)
    ↓ TCP
localhost:5000 (Simulator - Console App)
```

### Production (Proposed)
```
web.domain.com (Frontend - Static files on CDN/Nginx)
    ↓ HTTPS REST + WebSocket
api.domain.com (Backend - ASP.NET Core on IIS/Linux/Docker)
    ↓ TCP
192.168.1.100:502 (Real Hardware - Industrial Robot)
```

**Considerations:**
- Frontend as pre-built static files
- Backend requires .NET 9 runtime
- Hardware layer replaced with real robot TCP/IP
- WebSocket requires sticky sessions in load-balanced scenarios
- Use Redis backplane for SignalR scale-out

---

## Testing Strategy

### Backend Unit Tests (`RoboticControl.UnitTests`)
**Target:** Application layer business logic
**Framework:** MSTest + FluentAssertions + Moq
**Coverage:** 80%+
**Run:** Every commit and PR

**Tests:**
- `RobotControlService` - Work envelope validation, emergency stop priority
- `TcpRobotClient` - Protocol parsing, reconnection logic
- Validators - FluentValidation rule verification

### Backend Integration Tests (`RoboticControl.IntegrationTests`)
**Target:** API endpoints with hardware simulator
**Framework:** MSTest + WebApplicationFactory
**Run:** Every commit and PR

**Tests:**
- Full workflow: simulator → API → commands → responses
- SignalR hub communication
- Authentication/authorization flows

### Frontend Unit Tests (Planned)
**Target:** React components, hooks, services
**Framework:** Vitest + React Testing Library
**Run:** Every commit and PR

**Tests:**
- Component rendering and interactions
- Custom hooks (useRobotControl, useRobotStatus)
- API client mocking with MSW (Mock Service Worker)

### End-to-End Tests (Planned)
**Target:** Full user flows
**Framework:** Playwright/Cypress + Cucumber (BDD)
**Run:** Nightly, pre-release
**Environment:** Docker containers (backend + simulator)

**Tests:**
- Complete user flows (jog command, emergency stop)
- Real-time updates validation
- Error handling scenarios
- Cross-browser compatibility

---

## Project Structure

```
web_js_c_sharp/
├── src/                             # Backend (C#)
│   ├── RoboticControl.API/          # Web API + SignalR
│   │   ├── Controllers/
│   │   ├── Extensions/              # Configuration modules
│   │   ├── Hubs/
│   │   ├── BackgroundServices/
│   │   └── Middleware/
│   ├── RoboticControl.Application/  # Business logic
│   ├── RoboticControl.Domain/       # Core domain
│   └── RoboticControl.Infrastructure/ # Hardware communication
├── tests/                           # Backend tests
│   ├── RoboticControl.UnitTests/
│   └── RoboticControl.IntegrationTests/
├── tools/RobotSimulator/            # Mock hardware
├── client/                          # Frontend (React)
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       ├── hooks/
│       └── contexts/
├── start.ps1                        # Unified launcher
├── clean.ps1                        # Cleanup script
└── RoboticControl.sln              # Solution file
```

---

## Summary

This architecture provides a robust, maintainable, and scalable foundation for industrial robotic control systems. The clean separation of concerns, combined with modern patterns (DI, event-driven communication, real-time updates), creates a system that is both developer-friendly and production-ready.

**Key Strengths:**
- **Testability** - Clear layers enable easy unit/integration testing
- **Maintainability** - Clean Architecture keeps business logic framework-agnostic
- **Scalability** - Event-driven design supports multiple clients and horizontal scaling
- **Security** - JWT auth, rate limiting, input validation, CORS protection
- **Resilience** - Automatic reconnection, timeout handling, graceful degradation
- **Real-time** - SignalR provides sub-second position updates to all clients
- **Developer Experience** - Extension methods, consistent patterns, comprehensive documentation
