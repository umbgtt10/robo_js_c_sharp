# Robotic Control System

A production-grade full-stack web application for monitoring and controlling industrial robotic systems. Built with React SPA frontend, .NET 9 Web API backend, and TCP/IP hardware communication.

## 📋 Quick Links

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture, design patterns, and technical decisions
- **[ROADMAP.md](ROADMAP.md)** - Feature roadmap, future enhancements, and priorities
- **[tools/RobotSimulator/PROTOCOL.md](tools/RobotSimulator/PROTOCOL.md)** - Hardware communication protocol

## 🏗️ Technology Stack

### Frontend
- **React 18** with TypeScript
- **Vite** for build tooling
- **SignalR** for real-time bidirectional communication
- **Axios** for REST API calls
- **TailwindCSS** for styling

### Backend
- **.NET 9** Web API with Clean Architecture
- **SignalR** for WebSocket communication
- **Serilog** for structured logging
- **AutoMapper** for object mapping
- **FluentValidation** for input validation
- **JWT Bearer** authentication
- **Rate Limiting** for API protection

### Communication
- **REST API** for command and control
- **SignalR WebSockets** for real-time updates
- **TCP/IP** for hardware communication

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** and **npm** - [Download](https://nodejs.org/)
- Code editor (VS Code, Visual Studio, Rider)

### Quick Start

Use the launcher script to start all components:

**PowerShell:**
```powershell
.\start.ps1
```

**Command Prompt:**
```cmd
start.bat
```

**Options:**
```powershell
.\start.ps1 -Clean        # Clean build artifacts before starting
.\start.ps1 -SkipInstall  # Skip npm install
```

Once all services are running, open: **http://localhost:5173**

The script automatically starts:
1. Robot Simulator (port 5000)
2. Backend API (port 5001)
3. Frontend dev server (port 5173)

### Manual Start

#### 1. Start Robot Simulator
```bash
dotnet run --project tools/RobotSimulator
```

#### 2. Start Backend API
```bash
dotnet run --project src/RoboticControl.API
```

API runs on: `https://localhost:5001`

#### 3. Start Frontend
```bash
cd client
npm install  # First time only
npm run dev
```

Frontend runs on: `http://localhost:5173`

## 🧹 Cleanup

Remove all build artifacts and processes:

```powershell
.\clean.ps1
```

This removes:
- Running processes on ports 5000, 5001, 5173
- All `bin` and `obj` folders
- `node_modules` and `dist` folders
- `logs` folder

## 🎮 Using the Application

### Dashboard Features

**Position Display**
- Real-time X, Y, Z coordinates (mm)
- Rotation angles (degrees)
- Last update timestamp

**Jog Controls**
- Manual movement: ±X, ±Y, ±Z
- Step sizes: 1mm, 10mm, 100mm
- Validated against work envelope

**System Status**
- Robot state (Idle, Moving, EmergencyStopped)
- Temperature (°C) and load percentage
- Connection status
- Error codes and messages

**Emergency Stop**
- Large red circular button
- Immediately halts all operations
- Requires error reset to resume

### Testing the System

1. **Position Updates**: Watch real-time position changes
2. **Movement**: Click jog controls (+X, -X, +Y, -Y, +Z, -Z)
3. **Emergency Stop**: Click red button, then reset error
4. **Homing**: Click "🏠 Home Robot" button

## 📡 API Endpoints

Base URL: `https://localhost:5001/api`

### Robot Control
- `GET /robot/position` - Get current position
- `GET /robot/status` - Get system status
- `POST /robot/move` - Move to absolute position
- `POST /robot/jog` - Move relative (jog)
- `POST /robot/emergency-stop` - Emergency stop
- `POST /robot/home` - Execute homing
- `POST /robot/reset-error` - Reset error state

### Authentication
- `POST /auth/login` - User login (JWT)

### Configuration
- `GET /configuration/work-envelope` - Work envelope boundaries
- `GET /configuration/connection` - Connection settings

### Health
- `GET /health` - API health check

### Swagger Documentation

Interactive API docs: **https://localhost:5001/swagger** (Development only)

## ⚙️ Configuration

### Environment Variables

**JWT Secret (Production - REQUIRED):**
```bash
export JWT_SECRET_KEY="your-super-secret-production-key-min-32-chars"
```

**Hardware Connection (Optional):**
```bash
export HARDWARE__HOST="192.168.1.100"
export HARDWARE__PORT="502"
```

**CORS Origins (Optional):**
```bash
export CORS__ALLOWEDORIGINS__0="https://robotcontrol.example.com"
```

### Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

Configuration priority (lowest to highest):
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command-line arguments

### Development Credentials

- **Admin**: `admin` / `admin123`
- **Operator**: `operator` / `operator123`

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run unit tests
dotnet test tests/RoboticControl.UnitTests

# Run integration tests
dotnet test tests/RoboticControl.IntegrationTests

# Frontend tests
cd client
npm test
```

## 🔨 Building for Production

### Backend
```bash
dotnet publish src/RoboticControl.API -c Release -o ./publish
```

### Frontend
```bash
cd client
npm run build
```

Build output: `client/dist/`

## 🐛 Troubleshooting

### Launcher Script Issues

**Stopping services:**
- Press **Ctrl+C** in the terminal running `start.ps1`

**Execution policy error:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
Or use `start.bat` instead

**Port already in use:**
```powershell
.\clean.ps1
```

### Backend Issues

**.NET SDK not found:**
- Install .NET 9 SDK

**Port 5001 already in use:**
- Change port in `Properties/launchSettings.json` or stop other services

### Frontend Issues

**Cannot find module:**
```bash
cd client
npm install
```

**Port 5173 already in use:**
- Change port in `vite.config.ts`

### Connection Issues

**Simulator connection fails:**
- Ensure simulator is running on port 5000
- Check firewall settings

**SignalR not connecting:**
- Ensure backend API is running
- Check CORS settings in `appsettings.json`
- Verify proxy configuration in `vite.config.ts`

**Position not updating:**
- Check browser console for errors
- Check backend logs for hardware connection status
- Verify SignalR connection in Network tab

## 📂 Project Structure

```
web_js_c_sharp/
├── src/
│   ├── RoboticControl.API/          # Web API entry point
│   │   ├── Controllers/             # REST endpoints
│   │   ├── Extensions/              # Service configuration
│   │   ├── Hubs/                    # SignalR hubs
│   │   ├── BackgroundServices/      # Long-running services
│   │   └── Middleware/              # Custom middleware
│   ├── RoboticControl.Application/  # Business logic
│   │   ├── Services/                # Business services
│   │   ├── DTOs/                    # Data transfer objects
│   │   ├── Validators/              # FluentValidation rules
│   │   └── Mappings/                # AutoMapper profiles
│   ├── RoboticControl.Domain/       # Core domain
│   │   ├── Entities/                # Domain models
│   │   ├── Interfaces/              # Abstractions
│   │   ├── Enums/                   # Enumerations
│   │   └── Exceptions/              # Custom exceptions
│   └── RoboticControl.Infrastructure/ # External systems
│       ├── Hardware/                # TCP/IP communication
│       └── Configuration/           # Settings management
├── tests/
│   ├── RoboticControl.UnitTests/
│   └── RoboticControl.IntegrationTests/
├── tools/
│   └── RobotSimulator/              # Mock hardware server
├── client/                          # React SPA
│   └── src/
│       ├── components/              # UI components
│       ├── pages/                   # Route pages
│       ├── services/                # API & SignalR clients
│       ├── hooks/                   # Custom React hooks
│       └── contexts/                # React Context providers
├── start.ps1                        # Unified launcher
├── start.bat                        # Windows batch wrapper
├── clean.ps1                        # Cleanup script
├── README.md                        # This file
├── ARCHITECTURE.md                  # Architecture documentation
└── ROADMAP.md                       # Feature roadmap
```

## 🔒 Security Features

- **JWT Authentication** with BCrypt password hashing
- **Role-based Authorization** (Admin, Operator)
- **Rate Limiting** to prevent abuse
- **CORS** protection
- **Input Validation** with FluentValidation
- **Global Exception Handling**
- **Environment-specific Configuration** with secure secrets

## 📚 Learning Resources

This project demonstrates:
- Clean Architecture / Onion Architecture
- Dependency Injection
- Repository Pattern
- CQRS-like separation
- Event-driven communication
- Background services
- Real-time WebSocket communication
- React hooks and context
- TypeScript type safety

## 📝 License

This is a demonstration/educational application for showcasing full-stack web development with hardware integration.

## 🎯 Next Steps

See [ROADMAP.md](ROADMAP.md) for planned features and enhancements.

---

**Built with ❤️ for hightech manufacturing environments**

The application consists of three components: Robot Simulator, Backend API, and Frontend.

#### 🚀 Quick Start (Recommended)

Use the provided launcher script to start all components with one command:

**PowerShell:**
```powershell
.\start.ps1
```

**Command Prompt (double-click or run):**
```cmd
start.bat
```

**Options:**
```powershell
.\start.ps1 -Clean        # Clean build artifacts before starting
.\start.ps1 -SkipInstall  # Skip npm install (if dependencies already installed)
```

The script will:
- ✅ Automatically kill any existing processes on ports 5000, 5001, 5173
- ✅ Install frontend dependencies (if needed)
- ✅ Start the Robot Simulator (port 5000)
- ✅ Start the Backend API (port 5001)
- ✅ Start the Frontend dev server (port 5173)
- ✅ Monitor all processes
- ✅ Stop everything gracefully on **Ctrl+C**

Once all services are running, open your browser to:
🌐 **http://localhost:5173**

---

#### 📝 Manual Start (Alternative)

If you prefer to start components separately for debugging:

##### Step 1: Start the Robot Simulator

The simulator acts as mock hardware for development and testing.

```bash
dotnet run --project tools/RobotSimulator
```

You should see:
```
===================================
  Robotic Control System Simulator
===================================

Robot Simulator started on port 5000
```

**Keep this terminal open** ✅

##### Step 2: Start the Backend API

In a new terminal:

```bash
dotnet run --project src/RoboticControl.API
```

The API will start on:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000` (not recommended)

You should see:
```
info: RoboticControl.Infrastructure.Hardware.TcpRobotClient[0]
      Connecting to robot at localhost:5000
info: RoboticControl.Infrastructure.Hardware.TcpRobotClient[0]
      Successfully connected to robot
```

**Keep this terminal open** ✅

##### Step 3: Start the Frontend Development Server

In a new terminal:

```bash
cd client
npm run dev
```

The frontend will start on `http://localhost:5173`

You should see:
```
  VITE v5.1.3  ready in 500 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

**Keep this terminal open** ✅

##### Step 4: Open the Application

Open your browser and navigate to:

🌐 **http://localhost:5173**

You should see the Robotic Control Dashboard with:
- ✅ Green "Connected" status indicator
- ✅ Real-time position display
- ✅ System status panel
- ✅ Jog controls (movement buttons)
- ✅ Emergency stop button

## 🎮 Using the Application

### Dashboard Features

#### Position Display
Shows current robot position in real-time:
- X, Y, Z coordinates (mm)
- Rotation angles (degrees)
- Last update timestamp

#### Jog Controls
Manual robot movement:
- Select step size: 1mm, 10mm, or 100mm
- XY plane controls: Move in X and Y axes
- Z axis controls: Move up/down
- Movements are validated against work envelope

#### System Status
Displays:
- Robot state (Idle, Moving, EmergencyStopped, etc.)
- Temperature (°C)
- Load percentage
- Connection status
- Error codes and messages

#### Emergency Stop
- Large red circular button
- Immediately halts all robot operations
- Requires error reset to resume operations

#### Home Button
- Returns robot to home position (0, 0, 100, 0, 0, 0)
- Located in the header

### Testing the System

1. **Test Position Updates**:
   - Watch the position display update in real-time
   - Open browser console to see SignalR messages

2. **Test Movement**:
   - Click jog controls (+X, -X, +Y, -Y, +Z, -Z)
   - Position should update within 200-500ms
   - Status should change to "Moving" then back to "Idle"

3. **Test Emergency Stop**:
   - Click the large red EMERGENCY STOP button
   - All movements should halt
   - State changes to "EmergencyStopped"
   - Click "Reset Error State" to continue

4. **Test Homing**:
   - Click "🏠 Home Robot" button
   - Robot returns to (0, 0, 100)
   - Takes ~1 second to complete

## 📡 API Endpoints

### REST API

Base URL: `https://localhost:5001/api`

#### Robot Control

- `GET /robot/position` - Get current position
- `GET /robot/status` - Get system status
- `POST /robot/move` - Move to absolute position
- `POST /robot/jog` - Move relative (jog)
- `POST /robot/emergency-stop` - Emergency stop
- `POST /robot/home` - Execute homing sequence
- `POST /robot/reset-error` - Reset error state

#### Configuration

- `GET /configuration/work-envelope` - Get work envelope boundaries
- `GET /configuration/connection` - Get connection settings

#### Health

- `GET /health` - API health check

### SignalR Hub

WebSocket URL: `wss://localhost:5001/hubs/robot`

**Events from server:**
- `PositionUpdate` - Real-time position data (every 100ms)
- `StatusUpdate` - Real-time status changes

### Swagger Documentation

Interactive API documentation available at:

🔗 **https://localhost:5001/swagger**

## 🔧 Configuration

### Backend Configuration

Edit `src/RoboticControl.API/appsettings.json`:

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

### Frontend Configuration

Edit `client/vite.config.ts` to change API proxy settings:

```typescript
server: {
  port: 5173,
  proxy: {
    '/api': {
      target: 'https://localhost:5001',
      changeOrigin: true,
      secure: false,
    },
  },
}
```

## 🧪 Testing

### Run Unit Tests

```bash
dotnet test tests/RoboticControl.UnitTests
```

### Run Integration Tests

```bash
dotnet test tests/RoboticControl.IntegrationTests
```

### Run All Tests

```bash
dotnet test
```

## 🔨 Building for Production

### Build Backend

```bash
dotnet build -c Release
dotnet publish src/RoboticControl.API -c Release -o ./publish
```

### Build Frontend

```bash
cd client
npm run build
```

Build output will be in `client/dist/`

## 📚 Additional Documentation

- [PLAN.md](PLAN.md) - Comprehensive implementation plan and feature roadmap
- [tools/RobotSimulator/PROTOCOL.md](tools/RobotSimulator/PROTOCOL.md) - Hardware communication protocol specification

## 🛠️ Development

### Adding a New Robot Command

1. **Domain Layer**: Add enum value to `CommandType.cs`
2. **Infrastructure**: Implement command in `TcpRobotClient.cs` and `RobotHardwareService.cs`
3. **Application**: Add method to `RobotControlService.cs`
4. **API**: Add endpoint to `RobotController.cs`
5. **Frontend**: Add method to `apiClient.ts` and create UI in components

### Project Conventions

- **C# Naming**: PascalCase for public members, _camelCase for private fields
- **TypeScript Naming**: camelCase for variables/functions, PascalCase for components/types
- **Async Methods**: Suffix with `Async`
- **React Components**: One component per file, named exports
- **CSS**: Component-specific CSS files, use CSS variables

## 🐛 Troubleshooting

### Launcher Script Issues

- **Stopping all services**:
  - **Solution**: Press **Ctrl+C** in the terminal where `start.ps1` is running. All three processes will be stopped automatically.

- **Error**: `Execution policy error`
  - **Fix**: Run PowerShell as Administrator and execute: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
  - **Alternative**: Use `start.bat` instead, which bypasses execution policy

- **One service won't start**:
  - **Fix**: Run with `-Clean` flag to clear old artifacts: `.\start.ps1 -Clean`
  - **Fix**: Check individual component sections below for specific issues

- **Process already running on port**:
  - **Fix**: Stop existing processes manually or use Task Manager
  - **Fix**: Run `.\clean.ps1` and try again

### Backend won't start

- **Error**: `.NET SDK not found`
  - **Fix**: Install .NET 9 SDK from https://dotnet.microsoft.com/download

- **Error**: `Port 5001 already in use`
  - **Fix**: Change port in `Properties/launchSettings.json` or stop other services

### Frontend won't start

- **Error**: `Cannot find module`
  - **Fix**: Run `npm install` in the `client/` directory

- **Error**: `EADDRINUSE: port 5173 already in use`
  - **Fix**: Change port in `vite.config.ts` or stop other Vite instances

### Robot Simulator connection fails

- **Error**: `Connection refused`
  - **Fix**: Ensure simulator is running on port 5000
  - **Fix**: Check firewall settings

### SignalR not connecting

- **Error**: `Failed to start connection: Error: WebSocket failed to connect`
  - **Fix**: Ensure backend API is running
  - **Fix**: Check CORS settings in `appsettings.json`
  - **Fix**: Verify proxy configuration in `vite.config.ts`

### Position not updating

- **Check**: Browser console for errors
- **Check**: Backend logs for hardware connection status
- **Check**: SignalR connection status in Network tab (WebSocket)

## 📝 License

This project is a demonstration/educational application for showcasing full-stack web development with hardware integration.

## 👥 Contributing

This is a demo project, but suggestions and improvements are welcome!

## 🎯 Next Steps

See [PLAN.md](PLAN.md) for:
- Phase 2: Core Functionality (movement validation, safety features)
- Phase 3: Advanced Features (sequences, logging, 3D visualization)
- Phase 4: Production Readiness (authentication, testing, deployment)

---

**Built with for hightech manufacturing environments**
