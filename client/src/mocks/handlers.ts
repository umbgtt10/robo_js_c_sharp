import { http, HttpResponse } from 'msw';
import type { RobotPosition, RobotStatus, ApiResponse } from '../types';

// Simulated robot state
let currentPosition: RobotPosition = {
  x: 100,
  y: 50,
  z: 200,
  rotationX: 0,
  rotationY: 0,
  rotationZ: 0,
  timestamp: new Date().toISOString(),
};

let currentStatus: RobotStatus = {
  isConnected: true,
  state: 'Idle',
  temperature: 45.5,
  errorCode: 0,
  errorMessage: undefined,
  loadPercentage: 25,
  timestamp: new Date().toISOString(),
};

export const handlers = [
  // GET /api/robot/position
  http.get('/api/robot/position', () => {
    return HttpResponse.json(currentPosition);
  }),

  // GET /api/robot/status
  http.get('/api/robot/status', () => {
    return HttpResponse.json(currentStatus);
  }),

  // POST /api/robot/jog
  http.post('/api/robot/jog', async ({ request }) => {
    const body = await request.json() as any;

    // Update simulated position
    currentPosition = {
      ...currentPosition,
      x: currentPosition.x + (body.deltaX || 0),
      y: currentPosition.y + (body.deltaY || 0),
      z: currentPosition.z + (body.deltaZ || 0),
      timestamp: new Date().toISOString(),
    };

    // Check work envelope
    if (
      currentPosition.x < -1000 || currentPosition.x > 1000 ||
      currentPosition.y < -1000 || currentPosition.y > 1000 ||
      currentPosition.z < 0 || currentPosition.z > 500
    ) {
      return HttpResponse.json(
        {
          error: 'Movement exceeds work envelope',
        } as ApiResponse,
        { status: 400 }
      );
    }

    currentStatus = {
      ...currentStatus,
      state: 'Moving',
      timestamp: new Date().toISOString(),
    };

    // Simulate movement delay
    setTimeout(() => {
      currentStatus = {
        ...currentStatus,
        state: 'Idle',
        timestamp: new Date().toISOString(),
      };
    }, 100);

    return HttpResponse.json({
      success: true,
      message: 'Movement completed',
    } as ApiResponse);
  }),

  // POST /api/robot/move
  http.post('/api/robot/move', async ({ request }) => {
    const body = await request.json() as any;

    currentPosition = {
      x: body.x,
      y: body.y,
      z: body.z,
      rotationX: body.rotationX || 0,
      rotationY: body.rotationY || 0,
      rotationZ: body.rotationZ || 0,
      timestamp: new Date().toISOString(),
    };

    currentStatus = {
      ...currentStatus,
      state: 'Moving',
      timestamp: new Date().toISOString(),
    };

    setTimeout(() => {
      currentStatus = {
        ...currentStatus,
        state: 'Idle',
        timestamp: new Date().toISOString(),
      };
    }, 500);

    return HttpResponse.json({
      success: true,
      message: 'Movement completed',
    } as ApiResponse);
  }),

  // POST /api/robot/emergency-stop
  http.post('/api/robot/emergency-stop', () => {
    currentStatus = {
      ...currentStatus,
      state: 'EmergencyStopped',
      errorCode: 0,
      timestamp: new Date().toISOString(),
    };

    return HttpResponse.json({
      success: true,
      message: 'Emergency stop activated',
    } as ApiResponse);
  }),

  // POST /api/robot/home
  http.post('/api/robot/home', () => {
    currentPosition = {
      x: 0,
      y: 0,
      z: 100,
      rotationX: 0,
      rotationY: 0,
      rotationZ: 0,
      timestamp: new Date().toISOString(),
    };

    currentStatus = {
      ...currentStatus,
      state: 'Homing',
      timestamp: new Date().toISOString(),
    };

    setTimeout(() => {
      currentStatus = {
        ...currentStatus,
        state: 'Idle',
        timestamp: new Date().toISOString(),
      };
    }, 1000);

    return HttpResponse.json({
      success: true,
      message: 'Homing completed',
    } as ApiResponse);
  }),

  // POST /api/robot/reset-error
  http.post('/api/robot/reset-error', () => {
    currentStatus = {
      ...currentStatus,
      state: 'Idle',
      errorCode: 0,
      errorMessage: undefined,
      timestamp: new Date().toISOString(),
    };

    return HttpResponse.json({
      success: true,
      message: 'Error reset',
    } as ApiResponse);
  }),

  // POST /api/auth/login
  http.post('/api/auth/login', async ({ request }) => {
    const body = await request.json() as any;

    if (body.username === 'admin' && body.password === 'admin123') {
      return HttpResponse.json({
        token: 'mock-jwt-token-admin',
        username: 'admin',
        role: 'Admin',
        expiresAt: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(),
      });
    }

    if (body.username === 'operator' && body.password === 'operator123') {
      return HttpResponse.json({
        token: 'mock-jwt-token-operator',
        username: 'operator',
        role: 'Operator',
        expiresAt: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(),
      });
    }

    return HttpResponse.json(
      { error: 'Invalid credentials' },
      { status: 401 }
    );
  }),

  // GET /api/configuration/work-envelope
  http.get('/api/configuration/work-envelope', () => {
    return HttpResponse.json({
      xMin: -1000,
      xMax: 1000,
      yMin: -1000,
      yMax: 1000,
      zMin: 0,
      zMax: 500,
    });
  }),

  // GET /api/health
  http.get('/api/health', () => {
    return HttpResponse.json({
      status: 'healthy',
      timestamp: new Date().toISOString(),
    });
  }),
];
