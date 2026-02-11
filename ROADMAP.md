# Robotic Control System - Roadmap

## Completed Features ✅

### Phase 1: Core Infrastructure
- ✅ **React Frontend** - TypeScript, Vite, TailwindCSS
- ✅ **ASP.NET Core Backend** - .NET 9, REST API
- ✅ **Clean Architecture** - Domain/Application/Infrastructure/API layers
- ✅ **TCP Hardware Communication** - TcpRobotClient with simulator
- ✅ **SignalR Real-Time Updates** - WebSocket-based position/status broadcasting
- ✅ **Dependency Injection** - Service registration with proper lifetimes
- ✅ **Exception Handling** - GlobalExceptionHandler middleware

### Phase 2: Authentication & Security
- ✅ **JWT Authentication** - Bearer tokens with BCrypt hashing
- ✅ **Role-Based Authorization** - Admin/Operator policies
- ✅ **Rate Limiting** - 5 policies (global, auth, commands, queries, config)
- ✅ **CORS Configuration** - Environment-specific whitelisting
- ✅ **Secure Configuration** - Environment variables for secrets

### Phase 3: Validation & Data Integrity
- ✅ **FluentValidation** - Command validators with business rules
- ✅ **Work Envelope Validation** - Safe movement boundaries
- ✅ **AutoMapper** - Entity ↔ DTO mappings
- ✅ **Input Sanitization** - Client + server-side validation

### Phase 4: Logging & Monitoring
- ✅ **Serilog Integration** - Structured logging
- ✅ **File Logging** - Rotating daily logs
- ✅ **Console Logging** - Development debugging
- ✅ **Background Services** - Hardware connection + event broadcasting

### Phase 5: Resilience
- ✅ **Automatic Reconnection** - Exponential backoff
- ✅ **Connection Health Monitoring** - Disconnection detection
- ✅ **Thread-Safe Command Execution** - SemaphoreSlim queueing
- ✅ **Emergency Stop Priority** - Bypass normal queue

### Phase 6: Code Organization
- ✅ **Extension Methods** - Organized startup configuration
- ✅ **Service Separation** - AuthenticationExtensions, RateLimitingExtensions, etc.
- ✅ **Background Service Pattern** - Hardware initialization, event broadcasting
- ✅ **Middleware Pipeline** - Proper ordering and error handling

---

## In Progress 🔄

### Documentation Consolidation
- 🔄 **README.md** - Getting started + usage guide
- 🔄 **ARCHITECTURE.md** - Technical design + patterns
- 🔄 **ROADMAP.md** - Feature planning (this document)

---

## Must-Have Features (High Priority)

### Enhanced Health Checks
**Priority:** Critical
**Effort:** Medium
**Business Value:** Production readiness

**Features:**
- Health check endpoint (`/health`)
- Dependency checks (hardware connectivity, database if added)
- Custom health check for `RobotHardwareService`
- Health UI for monitoring (`/health-ui`)
- Integration with load balancers

**Acceptance Criteria:**
- Returns 200 OK when healthy, 503 Service Unavailable otherwise
- Shows individual component health (database, hardware, etc.)
- Timeout constraints (5-second max response)

---

### Correlation ID Middleware
**Priority:** High
**Effort:** Low
**Business Value:** Debugging and tracing

**Features:**
- Generate unique ID per request
- Include correlation ID in all logs
- Return ID in response headers (`X-Correlation-ID`)
- Propagate to external calls (hardware, future APIs)

**Acceptance Criteria:**
- Each request has unique identifier
- Full request trace in logs using correlation ID
- Client can reference ID for support issues

---

### Response Caching
**Priority:** Medium
**Effort:** Low
**Business Value:** Performance optimization

**Features:**
- Cache position/status queries
- Short TTL (500ms-1s) to reduce hardware calls
- Cache invalidation on command execution
- Memory cache for single-server deployments

**Acceptance Criteria:**
- Position queries don't hit hardware for every request
- Cache cleared on move/jog/home commands
- No stale data shown to users

---

### API Versioning
**Priority:** Medium
**Effort:** Medium
**Business Value:** API stability

**Features:**
- URL-based versioning (`/api/v1/robot`)
- Support multiple versions simultaneously
- Deprecation warnings in response headers
- Version-specific DTOs and controllers

**Acceptance Criteria:**
- `/api/v1/robot/position` and `/api/v2/robot/position` coexist
- Breaking changes don't affect existing clients
- Clear migration path documented

---

### Enhanced Error Responses
**Priority:** Medium
**Effort:** Low
**Business Value:** Developer experience

**Features:**
- RFC 7807 Problem Details format
- Error codes for client handling
- Validation error details (field-level)
- Stack traces only in development

**Acceptance Criteria:**
```json
{
  "type": "https://api.example.com/errors/validation",
  "title": "Validation Error",
  "status": 400,
  "detail": "Movement exceeds work envelope",
  "instance": "/api/v1/robot/move",
  "errors": {
    "X": ["Value 1200.0 exceeds maximum 1000.0"]
  }
}
```

---

## Should-Have Features (Medium Priority)

### Database Persistence
**Priority:** Medium
**Effort:** High
**Business Value:** Command history, analytics

**Features:**
- Entity Framework Core integration
- Movement history tracking
- User action auditing
- Error log storage
- PostgreSQL/SQL Server support

**Acceptance Criteria:**
- All commands logged to database
- Query movement history (`/api/history`)
- Audit trail for compliance
- Database migrations managed

---

### Metrics & Monitoring
**Priority:** Medium
**Effort:** Medium
**Business Value:** Observability

**Features:**
- OpenTelemetry integration
- Custom metrics (command rate, error rate, hardware uptime)
- Export to Prometheus/Application Insights
- Grafana dashboards

**Acceptance Criteria:**
- Metrics endpoint at `/metrics`
- Visualize command throughput over time
- Alert on hardware disconnections
- Performance bottleneck identification

---

### Unit & Integration Tests
**Priority:** High
**Effort:** High
**Business Value:** Code quality

**Backend Tests:**
- `RobotControlService` unit tests
- validator unit tests
- Controller integration tests
- SignalR hub tests

**Frontend Tests:**
- Component unit tests (Vitest + RTL)
- Hook tests (useRobotControl)
- API client mocking (MSW)

**Acceptance Criteria:**
- 80%+ code coverage
- Tests run in CI/CD pipeline
- Integration tests use WebApplicationFactory

---

### Advanced Work Envelope
**Priority:** Medium
**Effort:** Medium
**Business Value:** Safety

**Features:**
- Configurable work zones (safe, warning, forbidden)
- Collision detection with virtual obstacles
- User-defined restriction zones
- Visual work envelope in UI

**Acceptance Criteria:**
- Movements rejected outside safe zone
- Warning notifications in warning zone
- Configurable via API (`/api/configuration/workenvelope`)

---

### Command Queuing
**Priority:** Low
**Effort:** Medium
**Business Value:** Complex workflows

**Features:**
- Queue multiple commands
- Execute sequentially
- Pause/resume/cancel queue
- ETA calculation

**Acceptance Criteria:**
- POST `/api/robot/queue` with command array
- GET `/api/robot/queue` shows pending commands
- DELETE `/api/robot/queue` clears queue

---

## Nice-to-Have Features (Low Priority)

### Docker Support
**Priority:** Low
**Effort:** Medium
**Business Value:** Deployment simplicity

**Features:**
- Multi-stage Dockerfile for backend
- Dockerfile for frontend (Nginx)
- Docker Compose for local development
- Docker Compose for production

**Deliverables:**
```yaml
services:
  frontend:
    build: ./client
    ports: ["80:80"]
  backend:
    build: ./src/RoboticControl.API
    ports: ["5001:5001"]
    environment:
      - JWT_SECRET_KEY=${JWT_SECRET_KEY}
  simulator:
    build: ./tools/RobotSimulator
    ports: ["5000:5000"]
```

---

### User Management
**Priority:** Low
**Effort:** High
**Business Value:** Multi-user support

**Features:**
- User CRUD operations
- Password change/reset
- User roles (Admin, Operator, Viewer)
- Account lockout after failed logins
- Last login tracking

**Acceptance Criteria:**
- Admin can create/edit/delete users
- Users can change own password
- Role-based UI elements (hide admin features)

---

### Configuration UI
**Priority:** Low
**Effort:** Medium
**Business Value:** Ease of use

**Features:**
- Settings page in frontend
- Hardware connection settings
- Work envelope configuration
- User preferences (theme, units)
- Live configuration updates

**Acceptance Criteria:**
- Admin can change hardware IP/port without redeployment
- Configuration persisted to database
- Changes take effect immediately

---

### Multi-Robot Support
**Priority:** Low
**Effort:** Very High
**Business Value:** Scalability

**Features:**
- Multiple hardware connections
- Robot selection in UI
- Independent command queues
- Aggregated status dashboard

**Acceptance Criteria:**
- Support 1-10 robots simultaneously
- Each robot has dedicated SignalR group
- UI switches between robots

---

### Advanced Visualization
**Priority:** Low
**Effort:** High
**Business Value:** User experience

**Features:**
- 3D robot model (Three.js)
- Animated movement visualization
- Path trajectory preview
- Heatmap of frequently used positions

**Acceptance Criteria:**
- 3D model rotates/zooms
- Real-time position updates in 3D space
- Path visualization before execution

---

### Mobile App
**Priority:** Low
**Effort:** Very High
**Business Value:** Remote monitoring

**Features:**
- React Native app (iOS + Android)
- Real-time position monitoring
- Emergency stop button
- Push notifications for errors

**Acceptance Criteria:**
- Works on iOS 14+ and Android 10+
- Full feature parity with web UI
- Offline mode shows last known state

---

## Feature Prioritization Matrix

| Feature | Priority | Effort | Business Value | Dependencies |
|---------|----------|--------|----------------|--------------|
| Enhanced Health Checks | Critical | Medium | Production readiness | None |
| Correlation ID Middleware | High | Low | Debugging | None |
| Response Caching | Medium | Low | Performance | None |
| API Versioning | Medium | Medium | API stability | None |
| Enhanced Error Responses | Medium | Low | DX | None |
| Database Persistence | Medium | High | Analytics | None |
| Metrics & Monitoring | Medium | Medium | Observability | Health Checks |
| Unit & Integration Tests | High | High | Code quality | None |
| Advanced Work Envelope | Medium | Medium | Safety | None |
| Command Queuing | Low | Medium | Complex workflows | None |
| Docker Support | Low | Medium | Deployment | None |
| User Management | Low | High | Multi-user | Database |
| Configuration UI | Low | Medium | Ease of use | Database |
| Multi-Robot Support | Low | Very High | Scalability | Database, Advanced Architecture |
| Advanced Visualization | Low | High | UX | None |
| Mobile App | Low | Very High | Remote monitoring | API Versioning |

---

## Recommended Implementation Order

### Sprint 1-2: Production Essentials
1. Enhanced Health Checks
2. Correlation ID Middleware
3. Response Caching

### Sprint 3-4: Code Quality
4. Unit & Integration Tests (backend)
5. Enhanced Error Responses
6. API Versioning

### Sprint 5-6: Observability
7. Metrics & Monitoring
8. Database Persistence (foundation)

### Sprint 7-8: Advanced Features
9. Advanced Work Envelope
10. User Management (if database ready)
11. Command Queuing

### Sprint 9+: Optional Enhancements
12. Docker Support
13. Configuration UI
14. Multi-Robot Support (if needed)
15. Advanced Visualization
16. Mobile App (if justified)

---

## Technical Debt & Improvements

### Current Known Issues
- ❌ No frontend tests yet
- ❌ Limited error recovery scenarios
- ❌ No distributed tracing
- ❌ In-memory configuration only
- ❌ Single-robot limitation

### Code Quality Improvements
- 🔧 Add XML documentation comments to public APIs
- 🔧 Implement IAsyncDisposable where appropriate
- 🔧 Add nullable reference types throughout
- 🔧 Performance benchmarking
- 🔧 Security audit (OWASP Top 10)

---

## Long-Term Vision

**Year 1:** Production-ready single-robot system with full observability, comprehensive tests, and database persistence.

**Year 2:** Multi-robot support, advanced safety features, mobile app, and machine learning integration for predictive maintenance.

**Year 3:** Enterprise features (multi-tenancy, advanced analytics, API marketplace, third-party integrations).

---

## Contributing

To propose a new feature:
1. Check if it exists in this roadmap
2. Open an issue with the feature request template
3. Discuss business value, effort, and dependencies
4. Get approval before starting implementation

For bug fixes and small improvements, open a PR directly.

---

## Version History

- **v0.1.0** - Initial prototype (basic movement controls)
- **v0.2.0** - Added authentication and authorization
- **v0.3.0** - Rate limiting and enhanced validation
- **v0.4.0** - Extension methods refactoring + documentation consolidation
- **v1.0.0** (Target) - Production-ready with health checks, tests, and monitoring
