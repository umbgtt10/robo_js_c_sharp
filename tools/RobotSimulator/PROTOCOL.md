# Robot Simulator Protocol Documentation

## Overview

The Robot Simulator uses a text-based TCP/IP protocol for communication. Commands and responses are ASCII-encoded strings terminated with newline characters (`\n`).

## Connection

- **Protocol**: TCP/IP
- **Default Port**: 5000
- **Encoding**: ASCII

## Command Format

Commands follow the format:
```
COMMAND [parameters]\n
```

## Response Format

Responses follow one of two formats:
```
OK [data]\n
ERROR [message]\n
```

## Supported Commands

### MOVE_ABS

Move robot to absolute position.

**Format**: `MOVE_ABS x,y,z,rx,ry,rz`

**Parameters**:
- `x`: X coordinate in millimeters
- `y`: Y coordinate in millimeters
- `z`: Z coordinate in millimeters
- `rx`: Rotation around X axis in degrees
- `ry`: Rotation around Y axis in degrees
- `rz`: Rotation around Z axis in degrees

**Response**: `OK Movement completed` or `ERROR [message]`

**Example**:
```
Command:  MOVE_ABS 100.00,200.00,150.00,0.00,0.00,90.00
Response: OK Movement completed
```

---

### MOVE_REL

Move robot relative to current position.

**Format**: `MOVE_REL dx,dy,dz`

**Parameters**:
- `dx`: Delta X in millimeters
- `dy`: Delta Y in millimeters
- `dz`: Delta Z in millimeters

**Response**: `OK Movement completed` or `ERROR [message]`

**Example**:
```
Command:  MOVE_REL 10.00,0.00,-5.00
Response: OK Movement completed
```

---

### GET_POS

Get current robot position.

**Format**: `GET_POS`

**Response**: `OK x,y,z,rx,ry,rz`

**Example**:
```
Command:  GET_POS
Response: OK 100.00,200.00,150.00,0.00,0.00,90.00
```

---

### GET_STATUS

Get current system status.

**Format**: `GET_STATUS`

**Response**: `OK state,temperature,errorCode,loadPercentage`

**State Values**:
- `Idle`: Robot is idle and ready
- `Moving`: Robot is executing a movement
- `Homing`: Robot is executing homing sequence
- `EmergencyStopped`: Robot is in emergency stop state

**Example**:
```
Command:  GET_STATUS
Response: OK Idle,27.5,0,10
```

---

### STOP

Execute emergency stop.

**Format**: `STOP`

**Response**: `OK Emergency stop executed`

**Example**:
```
Command:  STOP
Response: OK Emergency stop executed
```

---

### HOME

Execute homing sequence (move to home position).

**Format**: `HOME`

**Response**: `OK Homing completed` or `ERROR [message]`

**Example**:
```
Command:  HOME
Response: OK Homing completed
```

**Note**: Homing moves robot to position (0, 0, 100, 0, 0, 0)

---

### RESET

Reset error state and clear emergency stop.

**Format**: `RESET`

**Response**: `OK Error reset`

**Example**:
```
Command:  RESET
Response: OK Error reset
```

---

## Error Handling

The simulator may return errors in various situations:

- **Communication timeout**: Random ~5% failure rate to simulate network issues
- **Emergency stop active**: Most commands will fail if robot is in emergency stop state
- **Invalid parameters**: Incorrect command format or parameter count
- **Unknown command**: Command not recognized

## Simulated Behavior

### Position Noise
`GET_POS` responses include small random noise (±0.1mm) to simulate real sensor readings.

### Temperature
Temperature fluctuates between 25-30°C and increases slightly during movement.

### Load Percentage
- Idle: ~10%
- Moving (absolute): ~75%
- Moving (relative): ~50%
- Homing: ~60%
- Emergency Stop: 0%

### Movement Duration
- Absolute movement: ~500ms
- Relative movement: ~200ms
- Homing: ~1000ms

## Usage

### Starting the Simulator

```bash
dotnet run --project tools/RobotSimulator
```

Or with custom port:

```bash
dotnet run --project tools/RobotSimulator -- 8000
```

### Testing with Telnet

```bash
telnet localhost 5000

# Enter commands:
GET_POS
MOVE_ABS 100,100,200,0,0,0
GET_STATUS
STOP
RESET
```

## Integration

When integrating with the API:

1. Start the simulator first
2. Configure API `appsettings.json` with simulator host/port
3. API will automatically connect and manage reconnection

## Notes

- All responses are newline-terminated
- Commands are case-insensitive
- Numeric values support decimal precision
- The simulator accepts multiple concurrent connections
- Emergency stop can always be executed, even during errors
