import { describe, it, expect, beforeEach } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '../../mocks/server';
import { apiClient } from '../apiClient';
import type { RobotPosition, RobotStatus, WorkEnvelope } from '../../types';

describe('apiClient', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('unwraps position data from the API response', async () => {
    const position: RobotPosition = {
      x: 1,
      y: 2,
      z: 3,
      rotationX: 0,
      rotationY: 0,
      rotationZ: 0,
      timestamp: new Date().toISOString(),
    };

    server.use(
      http.get('/api/robot/position', () => {
        return HttpResponse.json({ data: position });
      })
    );

    const result = await apiClient.getPosition();

    expect(result).toEqual(position);
  });

  it('unwraps status data from the API response', async () => {
    const status: RobotStatus = {
      isConnected: true,
      state: 'Idle',
      temperature: 22.5,
      errorCode: 0,
      errorMessage: undefined,
      loadPercentage: 10,
      timestamp: new Date().toISOString(),
    };

    server.use(
      http.get('/api/robot/status', () => {
        return HttpResponse.json({ data: status });
      })
    );

    const result = await apiClient.getStatus();

    expect(result).toEqual(status);
  });

  it('stores token and user details on login', async () => {
    server.use(
      http.post('/api/auth/login', () => {
        return HttpResponse.json({
          token: 'mock-token',
          username: 'admin',
          role: 'Admin',
          expiresAt: new Date().toISOString(),
        });
      })
    );

    const result = await apiClient.login('admin', 'admin123');

    expect(result.token).toBe('mock-token');
    expect(localStorage.getItem('authToken')).toBe('mock-token');
    expect(localStorage.getItem('user')).toBe(JSON.stringify({ username: 'admin', role: 'Admin' }));
  });

  it('unwraps work envelope data from the API response', async () => {
    const envelope: WorkEnvelope = {
      xMin: -1000,
      xMax: 1000,
      yMin: -500,
      yMax: 500,
      zMin: 0,
      zMax: 800,
    };

    server.use(
      http.get('/api/configuration/work-envelope', () => {
        return HttpResponse.json({ data: envelope });
      })
    );

    const result = await apiClient.getWorkEnvelope();

    expect(result).toEqual(envelope);
  });
});
