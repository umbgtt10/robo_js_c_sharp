# Robotic Control System - Architecture Documentation

## Overview

This is a three-tier web-based robotic control system built with React (frontend), ASP.NET Core (backend), and a TCP-based robot simulator (hardware layer). The system enables real-time monitoring and control of robotic hardware through a modern web interface.

---

## Three-Tier Architecture

### **1. Presentation Layer (Client - Port 5173)**

**Technology Stack**: React + TypeScript + Vite

**Key GUI Components**:
- `Dashboard.tsx` - Main container component, orchestrates the entire UI
- `PositionDisplay.tsx` - Real-time 3D position visualization
- `StatusPanel.tsx` - Connection status and system health monitoring
- `ControlPanel.tsx` - Manual control inputs for precise movements
- `MovementControls.tsx` - Jog controls for X/Y/Z axis movements

**Key Services**:
- `apiClient.ts` - REST API client for UI → Backend communication (Axios-based HTTP requests to Backend REST API, Request/Response)
- `signalRService.ts` - WebSocket client for Backend → UI communication (real-time push notifications over the WebSocket connection using SignalR, Event-driven updates)

**Responsibilities**:
- User interaction and input validation
- Real-time data visualization and updates
- WebSocket connection management (SignalR)
- State management using React hooks
- Responsive UI design with TailwindCSS

**Communication**:
- **UI → Backend**: HTTPS REST API calls via apiClient.ts (user-initiated actions)
- **Backend → UI**: SignalR WebSocket push notifications via signalRService.ts (real-time updates)

**Data Flow Diagram**:
┌─────────────────────────────────────────────────────────────┐
│                      RobotContext                           │
│  (Global State - Shared Across All Components)              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  State Variables:                                           │
│    • position: RobotPosition | null                         │
│    • status: RobotStatus | null                             │
│    • connectionStatus: ConnectionStatus                     │
│    • isLoading: boolean                                     │
│    • error: string | null                                   │
│                                                             │
└────┬──────────────────────────────────────────────────┬─────┘
     │                                                  │
     │ Initial Load                                     │ Real-Time Updates
     │ Manual Refresh                                   │ Automatic Push
     ▼                                                  ▼
┌──────────────────┐                            ┌─────────────────┐
│   apiClient.ts   │                            │ signalRService  │
│  (REST/HTTPS)    │                            │  (WebSocket)    │
└────────┬─────────┘                            └────────┬────────┘
         │                                               │
         │ HTTP Request                                  │ WebSocket
         │ GET /api/robot/position                       │ onPositionUpdate
         │ GET /api/robot/status                         │ onStatusUpdate
         ▼                                               ▼
┌──────────────────────────────────────────────────────────────┐
│              Backend API (Port 5001)                         │
│                                                              │
│  • REST Controller (responds to HTTP requests)               │
│  • SignalR Hub (broadcasts real-time updates)                │
│  • HardwareEventBroadcastService (relays hardware events)    │
└───────────────────────────────┬──────────────────────────────┘
                                │ TCP
                                ▼
┌──────────────────────────────────────────────────────────────┐
│           Robot Simulator (Port 5000)                        │
└──────────────────────────────────────────────────────────────┘


**React Core Hooks**

useState: Used for managing component local state
useEffect: Used to synchornize a compoenent with anexternal system (API calls, subscriptions, cleanup)
useCallback: used for memorizing functions to prevent unnecessary re-renders
useContext + createContext: used for global state management (avoiding prop drilling)

---

### **2. Application Layer (Backend API - Port 5001)**

**Technology Stack**: ASP.NET Core 9.0 + SignalR

**Project Structure** (Clean Architecture / Onion Architecture):

#### **API Layer** (`RoboticControl.API`)
Entry point and HTTP/WebSocket handlers

**Files**:
- `Program.cs` - Application startup and DI container configuration
- `Controllers/RobotController.cs` - REST API endpoints
  - `GET /api/robot/position` - Get current robot position (wired in the frontend and implemented on the backend)
  - `GET /api/robot/status` - Get robot connection and state (wired in the frontend and implemented on the backend)
  - `POST /api/robot/move` - Execute absolute move command (implemented on the backend but not wired in the frontend yet)
  - `POST /api/robot/jog` - Execute relative move command (jog by axis) (wired in the frontend and implemented on the backend)
  - `POST /api/robot/emergency-stop` - Emergency stop (wired in the frontend and implemented on the backend)
  - `POST /api/robot/home` - Execute homing sequence (implemented on the backend but not wired in the frontend yet)
  - `POST /api/robot/reset-error` - Reset error state (wired in the frontend and implemented on the backend)
- `Hubs/RobotHub.cs` - SignalR hub for server-to-client real-time push notifications (PositionUpdate, StatusUpdate)
- `BackgroundServices/` - Long-running hosted services
  - `HardwareConnectionService.cs` - Establishes and maintains hardware connection on startup
  - `HardwareEventBroadcastService.cs` - Subscribes to hardware events and broadcasts via SignalR

**Responsibilities**:
- HTTP request handling and routing
- Input validation and error handling (FluentValidation)
- Real-time event broadcasting via SignalR
- Background task orchestration

---

#### **Application Layer** (`RoboticControl.Application`)
Business logic and orchestration

**Files**:
- `Services/RobotControlService.cs` - Core business logic orchestration
  - Coordinates hardware operations
  - Implements business rules
  - Manages command execution flow
- `DTOs/` - Data Transfer Objects (API contracts)
  - `RobotPositionDto.cs` - Position data structure
  - `RobotStatusDto.cs` - Status information
  - `MoveCommandDto.cs` - Movement command parameters
- `Validators/` - FluentValidation rule sets
  - `JogCommandValidator.cs` - Validates jog commands (ensure only one axis is non-zero, within safe limits)
  - `MoveCommandValidator.cs` - Validates movement commands (range checks, safety limits)

- `Mappings/MappingProfile.cs` - AutoMapper configuration (Entity ↔ DTO transformations)

**Responsibilities**:
- Business rule enforcement
- Use case implementation
- Data transformation and validation
- Application workflow coordination

---

#### **Infrastructure Layer** (`RoboticControl.Infrastructure`)
External system integration and technical concerns

**Files**:
- `Hardware/TcpRobotClient.cs` - Low-level TCP/IP socket communication
  - Connection management
  - Command/response protocol implementation
  - Timeout and error handling
- `Hardware/RobotHardwareService.cs` - Hardware abstraction service
  - Connection lifecycle management
  - Automatic reconnection with exponential backoff
  - Event publishing (PositionChanged, StatusChanged)
  - Thread-safe command execution
- `Configuration/HardwareSettings.cs` - Configuration model
  - Host/Port settings
  - Timeout configurations
  - Reconnection policies

**Responsibilities**:
- External system integration (TCP/IP communication)
- Connection resilience and retry logic
- Configuration management
- Protocol translation (business operations → TCP commands)

---

#### **Domain Layer** (`RoboticControl.Domain`)
Core business entities and rules (framework-agnostic)

**Files**:
- `Entities/` - Core business entities
  - `RobotPosition.cs` - Position data (X, Y, Z, RotationX, RotationY, RotationZ)
  - `RobotStatus.cs` - Robot state and health information
- `Enums/` - Domain enumerations
  - `RobotState.cs` - States: Idle, Moving, Homing, EmergencyStopped, Error, Disconnected
  - `ErrorCode.cs` - Hardware error codes
- `Interfaces/IRobotHardwareService.cs` - Domain service contracts
- `Exceptions/HardwareConnectionException.cs` - Domain-specific exceptions

**Responsibilities**:
- Define business entities and value objects
- Encapsulate business rules
- Provide domain abstractions
- No dependencies on frameworks or infrastructure

---

### **3. Hardware Layer (Simulator - Port 5000)**

**Technology**: .NET Console Application with TCP Server

**Files**:
- `tools/RobotSimulator/Program.cs` - Entry point
- `tools/RobotSimulator/SimulatedRobotServer.cs` - TCP server implementation

**Communication Protocol**: Text-based command/response over TCP

**Supported Commands**:
```
MOVE_ABS x,y,z,rx,ry,rz  - Move to absolute position
MOVE_REL dx,dy,dz        - Move relative to current position
GET_POS                  - Get current position
GET_STATUS               - Get system status
STOP                     - Emergency stop
HOME                     - Execute homing sequence
RESET                    - Reset error state
```

**Response Format**:
```
OK <data>       - Success with optional data
ERROR <message> - Error with description
```

**Responsibilities**:
- Simulate physical robot hardware behavior
- Accept TCP socket connections
- Parse text-based commands
- Generate appropriate responses
- Maintain simulated position and state
- Introduce realistic motion delays

**Communication**: TCP Socket server ← Backend API client

---

## Key Architectural Patterns

### **1. Clean Architecture / Onion Architecture**

**Dependency Flow**:
```
API → Application → Domain
Infrastructure → Domain
```

**Layers** (inside-out):
- **Domain** (center) - Pure business logic, no external dependencies
- **Application** - Use cases and workflows, depends only on Domain
- **Infrastructure** - External concerns (TCP, file system), implements Domain interfaces
- **API** - Entry point and framework integration, orchestrates all layers

**Benefits**:
- Domain logic independent of frameworks and infrastructure
- Easy to test (mock infrastructure)
- Flexible to swap implementations
- Clear separation of concerns

---

### **2. Repository Pattern**

**Interface**: `IRobotHardwareService` (in Domain)
**Implementation**: `RobotHardwareService` (in Infrastructure)

**Benefits**:
- Abstraction over hardware communication
- Easy to swap real hardware for simulator
- Testability (mock hardware service)
- Encapsulates data access logic

---

### **3. Dependency Injection**

All services registered in `Program.cs` via built-in DI container:
```csharp
// Singleton - shared instance across application lifetime
builder.Services.AddSingleton<TcpRobotClient>();
builder.Services.AddSingleton<IRobotHardwareService, RobotHardwareService>();

// Scoped - instance per HTTP request
builder.Services.AddScoped<RobotControlService>();

// Hosted Services - background tasks
builder.Services.AddHostedService<HardwareConnectionService>();
builder.Services.AddHostedService<HardwareEventBroadcastService>();
```

**Benefits**:
- Loose coupling between components
- Easy unit testing (inject mocks)
- Lifetime management handled by framework
- Easy to swap implementations

---

### **4. CQRS-like Separation**

**Commands** (write operations):
- `MoveToPosition()` - Execute absolute movement
- `MoveRelative()` - Execute relative movement
- `EmergencyStop()` - Halt all motion
- `Home()` - Execute homing sequence
- `ResetError()` - Clear error state

**Queries** (read operations):
- `GetPosition()` - Read current position
- `GetStatus()` - Read system status

**Benefits**:
- Clear separation of read/write operations
- Different validation rules for commands vs queries
- Optimized data access patterns
- Foundation for future event sourcing

---

### **5. Event-Driven Communication**

**Events**:
- `PositionChanged` event - Fired when robot position updates
- `StatusChanged` event - Fired when robot status changes

**Event Flow**:
```
Hardware Service → Event → Position Polling Service → SignalR Hub → All Connected Clients
```

**Benefits**:
- Decoupled components
- Real-time UI updates without polling
- Scalable to multiple subscribers
- Reduced network traffic

---

### **6. Background Services / Hosted Services**

**Services**:
- `HardwareConnectionService` - Establishes initial hardware connection on startup
- `HardwareEventBroadcastService` - Subscribes to hardware events and broadcasts via SignalR

**Benefits**:
- Long-running tasks don't block HTTP requests
- Automatic lifecycle management
- Graceful shutdown handling
- Connection initialization before first request

---

## Data Flow Examples

### **Example 1: Jog Command Flow (Fully Implemented End-to-End)**

```
1. User clicks jog button in MovementControls component (e.g., "+X" button)
   - JogControls.tsx handles click event
   - Sets deltaX: 10, deltaY: 0, deltaZ: 0

2. Frontend calls apiClient.jog() → POST /api/robot/jog
   {
     "deltaX": 10,
     "deltaY": 0,
     "deltaZ": 0
   }

3. RobotController.Jog() receives HTTP request
   - Extracts JogCommandDto from request body

4. FluentValidation validates command (JogCommandValidator)
   - Ensure only one axis is non-zero (single-axis jog)
   - Check delta values are within safe limits (-100 to 100 mm)
   - Validation runs automatically in middleware pipeline

5. Controller calls RobotControlService.JogAsync(command)
   - Application layer business logic execution

6. RobotControlService.JogAsync() orchestrates workflow
   - Get current position from hardware service
   - Calculate resulting position: current + delta
   - Validate resulting position is within work envelope boundaries
   - If invalid, throw CommandValidationException
   - If valid, proceed to hardware layer

7. Calls IRobotHardwareService.MoveRelativeAsync(deltaX, deltaY, deltaZ)
   - Application layer delegates to infrastructure

8. RobotHardwareService.MoveRelativeAsync() handles hardware
   - Acquire command lock (SemaphoreSlim for thread safety)
   - Delegate to TcpRobotClient

9. TcpRobotClient.SendCommandAsync() executes TCP communication
   - Send: "MOVE_REL 10,0,0\n"
   - Wait for response with 5-second timeout
   - Parse response

10. Robot Simulator receives command
    - Parse "MOVE_REL 10,0,0"
    - Validate parameters
    - Calculate new position (currentX + 10)
    - Simulate motion delay (100ms)
    - Update internal position
    - Return: "OK"

11. Backend parses "OK" response
    - Release command lock
    - Return success to application layer

12. Hardware service fires PositionChanged event
    - Event data: RobotPosition { X: newX, Y, Z, ... }
    - Event propagates to subscribers

13. HardwareEventBroadcastService receives PositionChanged event
    - Maps RobotPosition (entity) → RobotPositionDto (DTO)
    - Calls SignalR hub context

14. SignalR Hub broadcasts to all connected clients
    - Clients.All.SendAsync("PositionUpdate", positionDto)
    - WebSocket protocol sends message to browser

15. Frontend SignalR client receives "PositionUpdate" message
    - signalRService.ts onPositionUpdate handler invoked
    - Updates RobotContext state via setPosition()

16. React components re-render with new position
    - PositionDisplay.tsx shows updated X/Y/Z coordinates
    - MovementControls.tsx enables next jog action
    - User sees position update in real-time
```

**Note**: The absolute move endpoint (`POST /api/robot/move`) is implemented on the backend but not yet wired in the frontend UI. The flow above represents the fully functional jog command that is actively used in the application.

---

### **Example 2: Real-Time Position Updates (Event-Driven)**

```
1. HardwareEventBroadcastService starts on application launch
   - Waits for hardware connection
   - Subscribes to PositionChanged and StatusChanged events

2. User executes a command or REST API endpoint is called
   - Backend calls hardware service method (e.g., GetCurrentPositionAsync())
   - Hardware service sends GET_POS command via TCP
   - Receives position data from simulator

3. Hardware service fires PositionChanged event
   - Event data: RobotPosition { X, Y, Z, RotationX, RotationY, RotationZ }

4. HardwareEventBroadcastService event handler executes
   - Maps RobotPosition → RobotPositionDto
   - Broadcasts via SignalR: hub.Clients.All.SendAsync("PositionUpdate", dto)

5. All connected browser clients receive WebSocket message
   - SignalR client invokes registered handler
   - React updates state via useState hook
   - PositionDisplay component re-renders with new values

6. User sees position update in UI immediately after command/query execution
```

---

### **Example 3: Emergency Stop Flow**

```
1. User clicks "Emergency Stop" button in UI

2. Frontend → POST /api/robot/emergency-stop

3. RobotController.EmergencyStop()
   - No validation required (emergency action)

4. RobotControlService.EmergencyStop()
   - Bypass normal command queue
   - Immediate execution

5. RobotHardwareService.EmergencyStopAsync()
   - Skip command lock (priority operation)
   - Send STOP command immediately

6. TcpRobotClient.SendCommandAsync("STOP\n")

7. Simulator receives STOP
   - Halt all motion immediately
   - Set state to EmergencyStopped
   - Return "OK"

8. Backend updates internal state
   - Fire StatusChanged event
   - New status: { State: EmergencyStopped, IsConnected: true }

9. SignalR broadcasts StatusUpdate to all clients

10. UI updates:
    - Status indicator turns red
    - "Emergency Stopped" message displayed
    - Move buttons disabled
    - Only "Reset" button enabled
```

---

## Cross-Cutting Concerns

### **Logging**

**Technology**: Serilog (structured logging)

**Configuration**:
- Console sink - Development debugging
- File sink - Rotating log files (`logs/robotcontrol-YYYYMMDD.txt`)
- Structured format - JSON-like logging with context

**Log Levels**:
- `Debug` - Detailed diagnostic information (command/response details)
- `Information` - General flow tracking (connections, movements)
- `Warning` - Recoverable errors (connection failures, retries)
- `Error` - Unhandled exceptions, critical failures

**Example Logs**:
```
[INF] Connecting to robot at localhost:5000
[INF] Successfully connected to robot
[DBG] Sent command: MOVE_ABS 100,50,200,0,0,0
[WRN] Failed to get robot position (connection lost)
[INF] Reconnection attempt 1 in 1000ms
```

---

### **Configuration Management**

**Files**:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development overrides

**Configuration Sections**:
```json
{
  "Hardware": {
    "Host": "localhost",
    "Port": 5000,
    "ConnectionTimeoutMs": 5000,
    "CommandTimeoutMs": 3000,
    "PollingIntervalMs": 100,
    "MaxReconnectAttempts": -1,
    "ReconnectDelayMs": 1000,
    "MaxReconnectDelayMs": 16000
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

**Binding**: Configuration sections bound to strongly-typed classes via Options pattern

---

### **Error Handling**

**Strategy**: Multi-layer error handling

**Domain Layer**:
- Custom exceptions: `HardwareConnectionException`
- Business rule violations throw domain exceptions

**Application Layer**:
- Catch infrastructure exceptions
- Transform to application-level errors
- Return appropriate DTOs to API layer

**API Layer**:
- Global exception handling middleware
- Controller try-catch blocks for specific actions
- Return appropriate HTTP status codes:
  - 200 OK - Success
  - 400 Bad Request - Validation errors
  - 500 Internal Server Error - Hardware failures
  - 503 Service Unavailable - Hardware disconnected

**Infrastructure Layer**:
- Retry logic with exponential backoff
- Connection timeout handling
- Graceful degradation

**Example Flow**:
```
Hardware disconnects → HardwareConnectionException thrown
  ↓
RobotHardwareService catches → Starts reconnection loop
  ↓
Returns error to RobotControlService
  ↓
Service returns error result to Controller
  ↓
Controller returns 503 Service Unavailable
  ↓
Frontend shows "Hardware Disconnected" status
  ↓
Backend automatically reconnects in background
  ↓
StatusChanged event fires → UI updates to "Connected"
```

---

### **Security**

**CORS Policy**:
- Whitelist allowed origins (frontend port 5173)
- Allow credentials (required for SignalR)
- Allow all headers and methods

**HTTPS**:
- Backend enforces HTTPS redirection
- Development certificate for local testing
- Production requires valid SSL certificate

**Input Validation**:
- Client-side validation (TypeScript types, React forms)
- Server-side validation (FluentValidation rules)
- Domain-level validation (entity invariants)

**Future Enhancements**:
- Authentication (JWT tokens)
- Authorization (role-based access control)
- Rate limiting (prevent command flooding)
- API key authentication for hardware connections

---

### **Resilience and Reliability**

**Connection Management**:
- Automatic reconnection with exponential backoff
- Maximum delay cap (16 seconds)
- Infinite retry attempts (configurable)
- Connection health monitoring

**Command Execution**:
- Thread-safe command queueing (SemaphoreSlim)
- Timeout handling (5 second default)
- Emergency stop bypasses normal queue

**State Management**:
- Disconnected state handling
- Error state recovery (RESET command)
- Graceful degradation (UI shows status)

**Monitoring**:
- Real-time status updates via SignalR
- Connection state visible in UI
- Background service health checks

---

## Technology Stack Summary

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TailwindCSS** - Utility-first styling
- **Axios** - HTTP client
- **SignalR Client** - WebSocket communication
- **Lucide React** - Icon library

### Backend
- **ASP.NET Core 9.0** - Web framework
- **SignalR** - Real-time communication
- **Serilog** - Structured logging
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **System.Net.Sockets** - TCP communication

### Hardware Simulator
- **.NET 9.0 Console** - Runtime
- **TcpListener** - TCP server
- **Text-based protocol** - Simple command/response

### Development Tools
- **PowerShell** - Automation scripts
- **.NET CLI** - Build and run
- **npm** - Frontend package management
- **Git** - Version control

---

## Deployment Architecture

### Development (Current Setup)
```
localhost:5173 (Frontend - Vite Dev Server)
    ↓ HTTPS
localhost:5001 (Backend - Kestrel Web Server)
    ↓ TCP
localhost:5000 (Simulator - Console App)
```

### Production (Possible Setup)
```
web.domain.com (Frontend - Static files on CDN/Nginx)
    ↓ HTTPS
api.domain.com (Backend - ASP.NET Core on IIS/Linux)
    ↓ TCP
192.168.1.100:5000 (Real Hardware - Industrial Robot Controller)
```

**Considerations**:
- Frontend can be pre-built and served as static files
- Backend requires .NET 9 runtime
- Hardware layer replaced with real robot TCP/IP interface
- WebSocket requires sticky sessions in load-balanced scenarios

---

## Startup Scripts

### `start.bat` / `start.ps1`
Unified launcher for all three services:

**Features**:
- Automatic port cleanup before starting
- Sequential startup with health checks
- Waits for each service port to be listening before starting next
- Monitors all processes
- Ctrl+C gracefully stops all services

**Startup Order**:
1. Run `clean.ps1 -ProcessesOnly` (stop any running instances)
2. Start Robot Simulator → Wait for port 5000 listening
3. Start Backend API → Wait for port 5001 listening
4. Start Frontend → Wait for port 5173 listening
5. Monitor all processes, restart on failure

### `clean.ps1`
Cleanup utility:

**Features**:
- `-ProcessesOnly` flag - Only stop processes, skip artifact deletion
- Kills processes by port (5000, 5001, 5173)
- Removes build artifacts (bin, obj, node_modules, dist, logs)
- Used by start script and manually for cleanup

---

## Future Enhancement Opportunities

### Features
- **Command History** - Log and replay command sequences
- **Position Presets** - Save favorite positions
- **Path Planning** - Define and execute multi-point trajectories
- **Collision Detection** - Virtual workspace boundaries
- **Multi-Robot Support** - Control multiple robots simultaneously

### Technical Improvements
- **Authentication** - User login with JWT tokens
- **Database** - Persist command history, positions, user preferences
- **Event Sourcing** - Full audit trail of all commands
- **Message Queue** - RabbitMQ/Redis for command queuing
- **Container Deployment** - Docker/Kubernetes orchestration
- **Health Checks** - ASP.NET Core health check endpoints
- **Metrics** - Prometheus/Grafana monitoring
- **End-to-End Tests** - Playwright/Cypress UI tests

### Scalability
- **Horizontal Scaling** - Multiple backend instances with load balancer
- **Redis Backplane** - SignalR scale-out for multiple servers
- **API Gateway** - Rate limiting, API versioning
- **Microservices** - Split into separate services (Command, Query, Hardware)

---

## File Structure Overview

```
web_js_c_sharp/
├── client/                          # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── components/             # React components
│   │   ├── services/               # API and SignalR clients
│   │   ├── types/                  # TypeScript type definitions
│   │   └── App.tsx                 # Main application component
│   ├── package.json
│   └── vite.config.ts
│
├── src/                            # Backend (ASP.NET Core)
│   ├── RoboticControl.API/         # Web API and SignalR
│   │   ├── Controllers/
│   │   ├── Hubs/
│   │   ├── BackgroundServices/
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── RoboticControl.Application/ # Business logic layer
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Mappings/
│   │
│   ├── RoboticControl.Infrastructure/ # External systems
│   │   ├── Hardware/
│   │   └── Configuration/
│   │
│   └── RoboticControl.Domain/      # Core domain
│       ├── Entities/
│       ├── Enums/
│       ├── Interfaces/
│       └── Exceptions/
│
├── tools/                          # Utilities
│   └── RobotSimulator/            # TCP hardware simulator
│       ├── Program.cs
│       └── SimulatedRobotServer.cs
│
├── start.ps1                       # Unified startup script
├── start.bat                       # Windows batch wrapper
├── clean.ps1                       # Cleanup script
├── global.json                     # .NET SDK version
├── RoboticControl.sln             # Solution file
└── README.md                       # Project documentation
```

## Testing strategy

1) Backend unit tests: Test business logic in Application layer with mocked hardware service. Full coverage. MsTest framework with FluentAssertions and Moq.
Run on every commit and pull request.
2) Backend integration tests: Test API endpoints with with hardware simulator. Validate end-to-end command execution and event broadcasting. MsTest framework with FluentAssertions and Moq. Run on every commit and pull request.
3) Frontend unit tests: Test React components, services and hooks with tools like MSW (Mock Service Library). Run on every commit and pull request.
4) Full End-to-end tests: Use Playwright or Cypress to automate browser interactions. Use Docker to run backend and simulator in test environment.
Use Cucumber or similar BDD framework to write user story-based tests.
Validate full user flows (e.g., jog command, emergency stop) with the backend and simulator running. Test real-time updates and error handling in the UI.
Run occasionally and during the night. Trace with requirements and user stories.




---

## Conclusion

This architecture provides a robust, maintainable, and scalable foundation for robotic control systems. The clean separation of concerns, combined with modern patterns like dependency injection, event-driven communication, and real-time updates, creates a system that is both developer-friendly and production-ready.

The layered approach ensures that business logic remains independent of frameworks and infrastructure, making it easy to test, modify, and extend the system as requirements evolve.
