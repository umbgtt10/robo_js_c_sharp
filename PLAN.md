# Implementation Plan: Full-Stack Robotic Control Web Application

**Project**: Robotic Control System with JavaScript SPA and C# Backend
**Created**: February 10, 2026
**Tech Stack**: React + TypeScript, .NET 9 Web API, SignalR, TCP/IP Hardware Communication

---

## Overview

A production-grade web application for monitoring and controlling an industrial robotic system. The React SPA provides a real-time dashboard for robot operations, communicating with a .NET 9 Web API backend that interfaces with the robotic hardware via TCP/IP. This architecture demonstrates enterprise patterns including clean architecture, real-time communication (SignalR), hardware abstraction, comprehensive testing, and security best practices suitable for hightech manufacturing environments.

### Key Architectural Decisions

- **React SPA with TypeScript** for type safety and maintainability
- **.NET 9 minimal API + controllers** for modern, performant backend
- **SignalR** for real-time bidirectional communication (robot status updates)
- **TCP/IP socket client** for hardware communication with reconnection logic
- **Clean Architecture** layers (API → Application → Domain → Infrastructure)
- **Simulated robot hardware server** for development and testing

---

## Proposed Functionalities

### Core Features (Must-Have)

1. **Real-time Robot Position Monitoring** - Display current X, Y, Z coordinates and rotation angles with live updates
2. **Manual Movement Control** - Jog controls for precise positioning (single axis movements)
3. **Pre-programmed Movement Sequences** - Execute predefined movement patterns (homing, calibration routes)
4. **Emergency Stop** - Immediate halt of all robot operations with visual/audio alerts
5. **Connection Status** - Monitor Web API and hardware connection health with automatic reconnection
6. **System Diagnostics** - Display robot status, errors, temperature, load metrics

### Advanced Features (Should-Have)

7. **Movement Queue Management** - Queue multiple commands with preview and cancellation
8. **Coordinate System Configuration** - Define work envelopes and coordinate transformations
9. **Session Logging** - Record all operations with timestamps for audit trails
10. **Historical Data Visualization** - Charts showing movement history, utilization, error rates
11. **User Authentication & Authorization** - Role-based access (Operator, Engineer, Admin)
12. **3D Workspace Visualization** - Interactive 3D view of robot position and movement paths
13. **Collision Detection Simulation** - Warn before movements that exceed boundaries
14. **Configuration Management** - Save/load robot configurations and movement profiles

### Enterprise Features (Nice-to-Have)

15. **Multi-Robot Support** - Manage multiple robot systems from single interface
16. **Predictive Maintenance Alerts** - Notify based on usage patterns and thresholds
17. **Integration API** - Webhook support for external system integration
18. **Mobile-Responsive Design** - Monitor operations on tablets/mobile devices
19. **Export & Reporting** - Generate PDF/CSV reports of operations
20. **Dark Mode** - UI theme switching for different lighting conditions

---

## Project Structure

```
c:\Projects\web_js_c_sharp\
├── RoboticControl.sln
├── README.md
├── docker-compose.yml
├── Dockerfile
├── .gitignore
│
├── src/
│   ├── RoboticControl.API/                  # Web API entry point
│   │   ├── Controllers/                     # REST endpoints
│   │   ├── Hubs/                           # SignalR hubs
│   │   ├── BackgroundServices/             # Polling services
│   │   ├── Program.cs                      # App configuration
│   │   └── appsettings.json
│   │
│   ├── RoboticControl.Application/          # Business logic
│   │   ├── Services/                       # Business services
│   │   ├── DTOs/                           # Data transfer objects
│   │   ├── Validators/                     # Input validation
│   │   └── Mappings/                       # AutoMapper profiles
│   │
│   ├── RoboticControl.Domain/               # Core domain
│   │   ├── Entities/                       # Domain models
│   │   ├── Interfaces/                     # Abstractions
│   │   ├── Enums/                          # Enumerations
│   │   └── Exceptions/                     # Custom exceptions
│   │
│   └── RoboticControl.Infrastructure/       # External concerns
│       ├── Hardware/                       # TCP/IP communication
│       ├── Persistence/                    # Database context
│       └── Configuration/                  # Settings management
│
├── tests/
│   ├── RoboticControl.UnitTests/           # Unit tests
│   └── RoboticControl.IntegrationTests/    # Integration tests
│
├── tools/
│   └── RobotSimulator/                     # Mock hardware server
│       ├── SimulatedRobotServer.cs
│       ├── PROTOCOL.md
│       └── RobotSimulator.csproj
│
├── client/                                  # React SPA
│   ├── public/
│   ├── src/
│   │   ├── components/                     # React components
│   │   │   ├── Layout/
│   │   │   ├── RobotControl/
│   │   │   ├── Sequences/
│   │   │   └── Visualization/
│   │   ├── pages/                          # Route pages
│   │   ├── services/                       # API + SignalR clients
│   │   ├── contexts/                       # React Context
│   │   ├── hooks/                          # Custom hooks
│   │   ├── types/                          # TypeScript types
│   │   ├── styles/                         # Global styles
│   │   ├── App.tsx                         # Root component
│   │   └── main.tsx                        # Entry point
│   ├── e2e/                                # Playwright tests
│   ├── package.json
│   ├── vite.config.ts
│   └── tsconfig.json
│
└── docs/
    ├── DEVELOPER_GUIDE.md
    ├── HARDWARE_PROTOCOL.md
    └── ARCHITECTURE.md
```

---

## Implementation Steps

### Phase 1: Project Structure & Skeleton (Foundation)

#### 1.1 Backend Project Structure

- Create solution file `RoboticControl.sln`
- Create API project `src/RoboticControl.API/RoboticControl.API.csproj` - Web API entry point
- Create Application layer `src/RoboticControl.Application/RoboticControl.Application.csproj` - Business logic, services
- Create Domain layer `src/RoboticControl.Domain/RoboticControl.Domain.csproj` - Entities, interfaces, enums
- Create Infrastructure layer `src/RoboticControl.Infrastructure/RoboticControl.Infrastructure.csproj` - Hardware communication, data access
- Create test projects `tests/RoboticControl.UnitTests/` and `tests/RoboticControl.IntegrationTests/`

#### 1.2 Backend Core Configuration

- Configure `src/RoboticControl.API/Program.cs` with dependency injection, CORS, SignalR, Swagger
- Create `src/RoboticControl.API/appsettings.json` and `appsettings.Development.json` with hardware connection settings
- Set up logging with Serilog in `src/RoboticControl.API/Program.cs`
- Configure global exception handling middleware

#### 1.3 Domain Layer - Core Models

- Create `RobotPosition` entity in `src/RoboticControl.Domain/Entities/RobotPosition.cs` (X, Y, Z, RotationX, RotationY, RotationZ, Timestamp)
- Create `RobotCommand` entity in `src/RoboticControl.Domain/Entities/RobotCommand.cs` (CommandType, Parameters, Status)
- Create `RobotStatus` entity in `src/RoboticControl.Domain/Entities/RobotStatus.cs` (IsConnected, State, Temperature, ErrorCode)
- Create enums in `src/RoboticControl.Domain/Enums/` (RobotState, CommandType, ErrorCode, ConnectionStatus)
- Define `IRobotHardwareService` interface in `src/RoboticControl.Domain/Interfaces/IRobotHardwareService.cs`

#### 1.4 Infrastructure Layer - Hardware Communication

- Implement `TcpRobotClient` class in `src/RoboticControl.Infrastructure/Hardware/TcpRobotClient.cs`
  - TCP/IP socket connection with auto-reconnection logic
  - Send commands and receive responses with timeout handling
  - Protocol parsing (assume custom binary or text-based protocol)
- Implement `RobotHardwareService` in `src/RoboticControl.Infrastructure/Hardware/RobotHardwareService.cs` implementing `IRobotHardwareService`
  - Command queue management
  - Position polling with configurable interval
  - Event-based status updates
- Create connection factory in `src/RoboticControl.Infrastructure/Hardware/RobotConnectionFactory.cs`

#### 1.5 Application Layer - Business Logic

- Create `RobotControlService` in `src/RoboticControl.Application/Services/RobotControlService.cs`
  - `MoveToPosition(x, y, z)` with validation
  - `ExecuteEmergencyStop()` with priority handling
  - `GetCurrentPosition()` with caching
  - `GetSystemStatus()` aggregation
- Create DTOs in `src/RoboticControl.Application/DTOs/` (RobotPositionDto, RobotStatusDto, MoveCommandDto)
- Implement AutoMapper profiles for entity-to-DTO mapping in `src/RoboticControl.Application/Mappings/MappingProfile.cs`
- Create fluent validation in `src/RoboticControl.Application/Validators/` for command inputs (range checks, work envelope validation)

#### 1.6 API Layer - Controllers & Endpoints

- Create `RobotController` in `src/RoboticControl.API/Controllers/RobotController.cs`
  - GET `/api/robot/position` - current position
  - GET `/api/robot/status` - system status
  - POST `/api/robot/move` - execute movement command
  - POST `/api/robot/emergency-stop` - halt operations
  - POST `/api/robot/home` - execute homing sequence
- Create `ConfigurationController` in `src/RoboticControl.API/Controllers/ConfigurationController.cs`
  - GET/PUT `/api/configuration/work-envelope` - workspace boundaries
  - GET/PUT `/api/configuration/connection` - hardware connection settings
- Implement SignalR hub `RobotHub` in `src/RoboticControl.API/Hubs/RobotHub.cs`
  - Push position updates every 100ms
  - Push status changes immediately
  - Handle client subscriptions

#### 1.7 Frontend Project Structure

- Initialize React project with Vite in `client/`
- Create `client/package.json` with dependencies: axios, @microsoft/signalr, recharts, react-router-dom
- Set up TypeScript configuration in `client/tsconfig.json`
- Configure Vite proxy in `client/vite.config.ts` to redirect API calls to backend
- Set up ESLint and Prettier for code quality in `client/.eslintrc.js` and `client/.prettierrc`

#### 1.8 Frontend Core Architecture

- Create API client service in `client/src/services/apiClient.ts` (axios instance with base URL, interceptors)
- Create SignalR service in `client/src/services/signalRService.ts` (connection management, event subscriptions)
- Set up React Context for robot state in `client/src/contexts/RobotContext.tsx` (position, status, connection state)
- Create custom hooks in `client/src/hooks/`:
  - `useRobotPosition.ts` - real-time position updates via SignalR
  - `useRobotControl.ts` - command sending with loading states
  - `useRobotStatus.ts` - system status monitoring
- Define TypeScript interfaces in `client/src/types/` matching backend DTOs

#### 1.9 Frontend UI Components (Skeleton)

- Create layout components in `client/src/components/Layout/`:
  - `Header.tsx` - app title, connection status indicator
  - `Sidebar.tsx` - navigation menu
  - `MainLayout.tsx` - overall page structure
- Create robot control components in `client/src/components/RobotControl/`:
  - `PositionDisplay.tsx` - current X, Y, Z, rotation values
  - `JogControls.tsx` - directional movement buttons (±X, ±Y, ±Z)
  - `EmergencyStopButton.tsx` - prominent red stop button
  - `StatusPanel.tsx` - connection state, robot state, errors
- Create routing in `client/src/App.tsx` with react-router-dom:
  - `/` - Dashboard (position + controls)
  - `/history` - Movement history placeholder
  - `/settings` - Configuration placeholder
- Create global styles in `client/src/styles/` (CSS variables for theming, responsive breakpoints)

#### 1.10 Simulated Hardware Server (Development Tool)

- Create standalone console app `tools/RobotSimulator/RobotSimulator.csproj`
- Implement TCP server in `tools/RobotSimulator/SimulatedRobotServer.cs`
  - Listen on configurable port (default 5000)
  - Simulate robot position with physics (acceleration, velocity limits)
  - Respond to protocol commands (MOVE, GET_POS, STOP, HOME)
  - Inject random errors for testing (5% failure rate, timeout simulation)
- Create protocol documentation in `tools/RobotSimulator/PROTOCOL.md`

---

### Phase 2: Core Functionality Implementation

#### 2.1 Real-Time Position Monitoring

- Implement background service `PositionPollingService` in `src/RoboticControl.API/BackgroundServices/PositionPollingService.cs`
  - Poll hardware every 100ms via `IRobotHardwareService`
  - Broadcast position updates via `RobotHub`
  - Handle disconnections gracefully
- Update `RobotHub` to manage active connections and selective broadcasting
- Implement SignalR reconnection logic in `client/src/services/signalRService.ts` with exponential backoff
- Update `PositionDisplay.tsx` to show live values with visual update indicators (flash on change)
- Add position history chart in `client/src/components/PositionChart.tsx` using Recharts (last 50 points)

#### 2.2 Manual Movement Control

- Implement movement validation in `RobotControlService`:
  - Check work envelope boundaries
  - Validate speed/acceleration limits
  - Prevent conflicting commands
- Add command queueing with FIFO execution in `RobotHardwareService`
- Implement optimistic UI updates in `JogControls.tsx` (instant visual feedback, rollback on failure)
- Add movement step size selector (1mm, 10mm, 100mm) in `client/src/components/RobotControl/StepSizeSelector.tsx`
- Create coordinate input form in `client/src/components/RobotControl/PositionInput.tsx` for direct position entry

#### 2.3 Emergency Stop & Safety

- Implement emergency stop in `RobotHardwareService` with highest priority (bypass queue)
- Add TCP keep-alive monitoring with automatic emergency stop on connection loss
- Create prominent emergency stop button in `EmergencyStopButton.tsx` with confirmation modal
- Implement safety interlocks in `RobotControlService` (prevent movement in error states)
- Add visual/audio alerts in `client/src/components/AlertSystem.tsx` using browser notifications API

#### 2.4 Connection Management

- Implement connection health check in `src/RoboticControl.API/HealthChecks/HardwareHealthCheck.cs`
- Add `/health` endpoint with detailed hardware status
- Create connection status indicator in `Header.tsx` (green/yellow/red with tooltip)
- Implement automatic reconnection in `TcpRobotClient` with exponential backoff (1s, 2s, 4s, 8s, 16s max)
- Add manual reconnect button in `client/src/components/ConnectionPanel.tsx`

---

### Phase 3: Advanced Features

#### 3.1 Pre-Programmed Sequences

- Create `MovementSequence` entity in `src/RoboticControl.Domain/Entities/MovementSequence.cs`
- Implement `SequenceExecutionService` in `src/RoboticControl.Application/Services/SequenceExecutionService.cs`
  - Load sequence definitions from JSON
  - Execute waypoints sequentially with interpolation
  - Support pause/resume/cancel
- Add `SequencesController` in `src/RoboticControl.API/Controllers/SequencesController.cs`
- Create sequence management UI in `client/src/components/Sequences/`
  - `SequenceList.tsx` - available sequences
  - `SequencePlayer.tsx` - execution controls with progress bar
  - `SequenceEditor.tsx` - create/edit sequences (stretch goal)

#### 3.2 Historical Data & Logging

- Integrate Entity Framework Core in Infrastructure layer
- Create `RobotOperationLog` entity in `src/RoboticControl.Domain/Entities/RobotOperationLog.cs`
- Implement `LoggingService` in `src/RoboticControl.Application/Services/LoggingService.cs` to persist all commands and status changes
- Add SQLite database context in `src/RoboticControl.Infrastructure/Persistence/RobotControlDbContext.cs`
- Create `HistoryController` with pagination in `src/RoboticControl.API/Controllers/HistoryController.cs`
- Build history view in `client/src/pages/HistoryPage.tsx` with filtering by date, command type, status
- Add data export functionality (CSV download) in `client/src/components/History/ExportButton.tsx`

#### 3.3 Configuration Management

- Create settings persistence in `src/RoboticControl.Infrastructure/Configuration/SettingsRepository.cs`
- Implement work envelope validation in `RobotControlService`
- Build settings page in `client/src/pages/SettingsPage.tsx`:
  - Hardware connection settings (IP, port, timeout)
  - Work envelope boundaries (X min/max, Y min/max, Z min/max)
  - Movement parameters (max speed, acceleration)
  - UI preferences (refresh rate, units, theme)
- Add settings import/export (JSON format)

#### 3.4 3D Visualization (Optional)

- Integrate Three.js or React Three Fiber in `client/src/components/Visualization/`
- Create `RobotVisualization3D.tsx` component showing:
  - 3D representation of robot (simple box/arm model)
  - Work envelope boundaries as wireframe box
  - Current position marker
  - Movement path trail (last N positions)
- Add camera controls (orbit, zoom, pan) using Three.js OrbitControls
- Display visualization in dashboard as optional panel

---

### Phase 4: Quality, Security & Deployment

#### 4.1 Unit Testing

- Write tests for `RobotControlService` in `tests/RoboticControl.UnitTests/Services/RobotControlServiceTests.cs`
  - Test work envelope validation
  - Test emergency stop priority
  - Mock `IRobotHardwareService` using Moq
- Write tests for `TcpRobotClient` in `tests/RoboticControl.UnitTests/Infrastructure/TcpRobotClientTests.cs`
  - Test protocol parsing
  - Test reconnection logic
  - Mock TcpClient
- Target 80% code coverage for Application and Infrastructure layers

#### 4.2 Integration Testing

- Create integration test harness in `tests/RoboticControl.IntegrationTests/`
- Use WebApplicationFactory to spin up API in tests
- Test full workflow: start simulator → connect API → send commands → verify responses
- Test SignalR hub communication
- Create `TestRobotSimulator` class for in-process mock hardware

#### 4.3 Frontend Testing

- Set up Vitest in `client/vitest.config.ts`
- Write component tests in `client/src/components/**/*.test.tsx` using React Testing Library
- Test critical components: `JogControls`, `EmergencyStopButton`, `PositionDisplay`
- Mock SignalR service for isolated component testing
- Add E2E testing skeleton with Playwright in `client/e2e/`

#### 4.4 Authentication & Authorization (Basic)

- Integrate JWT authentication in `src/RoboticControl.API/Authentication/`
- Create user roles (Operator, Engineer, Admin) in `src/RoboticControl.Domain/Enums/UserRole.cs`
- Add `[Authorize]` attributes to controllers with role requirements
- Implement login endpoint in `AuthController`
- Create login page in `client/src/pages/LoginPage.tsx`
- Store JWT in localStorage with automatic refresh
- Add role-based UI element visibility (hide admin features from operators)

#### 4.5 Error Handling & Logging

- Implement global exception filter in `src/RoboticControl.API/Filters/GlobalExceptionFilter.cs`
- Create custom exceptions in `src/RoboticControl.Domain/Exceptions/` (HardwareConnectionException, CommandValidationException)
- Configure Serilog with structured logging to file and console
- Add request/response logging middleware
- Implement error boundary in React `client/src/components/ErrorBoundary.tsx`
- Create user-friendly error messages in `client/src/components/ErrorDisplay.tsx`

#### 4.6 Documentation

- Create `README.md` with:
  - Project overview and architecture diagram
  - Prerequisites and setup instructions
  - How to run (simulator → backend → frontend)
  - API documentation link
- Generate Swagger/OpenAPI documentation (auto-configured in Program.cs)
- Create developer guide in `docs/DEVELOPER_GUIDE.md`:
  - Project structure explanation
  - How to add new robot commands
  - Testing strategy
  - Deployment process
- Create protocol documentation in `docs/HARDWARE_PROTOCOL.md`
- Add inline XML documentation to all public classes and methods

#### 4.7 Deployment Configuration

- Create Docker support:
  - `Dockerfile` for backend API
  - `docker-compose.yml` orchestrating API + simulator + frontend
- Add CI/CD pipeline configuration in `.github/workflows/ci.yml`:
  - Build backend and frontend
  - Run unit and integration tests
  - Code quality checks (SonarQube/CodeQL)
  - Docker image build and push
- Create production configuration in `src/RoboticControl.API/appsettings.Production.json`
- Add environment variable configuration documentation

---

## Verification Strategy

### Development Workflow

1. Start robot simulator: `dotnet run --project tools/RobotSimulator`
2. Start backend API: `dotnet run --project src/RoboticControl.API` (runs on https://localhost:5001)
3. Start frontend dev server: `cd client && npm run dev` (runs on http://localhost:5173)
4. Open browser to http://localhost:5173

### Testing

- Run backend tests: `dotnet test`
- Run frontend tests: `cd client && npm test`
- Check code coverage: `dotnet test /p:CollectCoverage=true`
- E2E tests: `cd client && npm run test:e2e`

### Manual Testing Checklist

- [ ] Connection indicator shows green when all systems running
- [ ] Position display updates in real-time (verify in console that updates arrive every 100ms)
- [ ] Jog controls move robot and UI reflects new position within 500ms
- [ ] Emergency stop immediately halts movement and displays warning
- [ ] Disconnecting simulator triggers reconnection attempts (check logs)
- [ ] Work envelope validation prevents out-of-bounds movements
- [ ] Pre-programmed sequence executes all waypoints correctly
- [ ] Historical data persists across application restarts
- [ ] Configuration changes save and apply correctly

### Production Readiness

- [ ] All tests passing (unit, integration, E2E)
- [ ] Code coverage >80% for backend
- [ ] No critical Swagger warnings
- [ ] Dockerfile builds successfully
- [ ] Docker Compose orchestration works end-to-end
- [ ] Documentation complete and accurate
- [ ] Security review completed (authentication, CORS, input validation)

---

## Technical Decisions

**React over Angular/Vue**: React offers the largest ecosystem for hardware visualization libraries (Three.js integrations), better performance for real-time updates via virtual DOM, and more available component libraries for industrial UIs.

**SignalR over WebSockets**: SignalR provides automatic reconnection, multiple transport fallbacks (WebSockets → Server-Sent Events → Long Polling), and type-safe client generation, reducing boilerplate compared to raw WebSocket implementation.

**TCP/IP over Serial**: TCP/IP allows remote hardware access, easier testing with simulated server, and network-based deployment flexibility. Serial port communication would require hardware on the same machine and complicate development/testing.

**Clean Architecture**: Separating Domain, Application, Infrastructure, and API layers ensures testability, allows hardware abstraction swapping (mock vs. real), and follows industry best practices for enterprise applications.

**SQLite for Development**: Lightweight, zero-config database suitable for logging and configuration persistence. Easily upgradable to PostgreSQL/SQL Server for production via EF Core provider swap.

**Minimal API + Controllers**: Use .NET 9's minimal APIs for simple endpoints (health checks) while leveraging controllers for complex robot operations that benefit from model binding, validation, and OpenAPI generation.

---

## Status

This plan is comprehensive and ready for execution. The skeleton phase (Phase 1) creates a fully functional but minimal application demonstrating all architectural layers. Phases 2-4 add features incrementally, maintaining a working application at each step.
