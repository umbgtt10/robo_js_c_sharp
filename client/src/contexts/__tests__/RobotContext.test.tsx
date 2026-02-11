import { describe, it, expect, beforeEach, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { render } from '../../test/test-utils';
import { useRobot } from '../RobotContext';
import type { RobotPosition, RobotStatus } from '../../types';
import { apiClient } from '../../services/apiClient';
import { signalRService } from '../../services/signalRService';

const apiClientMock = vi.hoisted(() => ({
  getPosition: vi.fn(),
  getStatus: vi.fn(),
}));

const signalRMock = vi.hoisted(() => ({
  connect: vi.fn(),
  onPositionUpdate: vi.fn(),
  onStatusUpdate: vi.fn(),
  disconnect: vi.fn(),
}));

vi.mock('../../services/apiClient', () => ({
  apiClient: apiClientMock,
}));

vi.mock('../../services/signalRService', () => ({
  signalRService: signalRMock,
  default: signalRMock,
}));

function TestConsumer() {
  const { connectionStatus, isLoading, error, position, status } = useRobot();
  return (
    <div>
      <div data-testid="connection">{connectionStatus}</div>
      <div data-testid="loading">{isLoading ? 'loading' : 'done'}</div>
      <div data-testid="error">{error ?? ''}</div>
      <div data-testid="position">{position ? 'position-loaded' : 'position-missing'}</div>
      <div data-testid="status">{status ? 'status-loaded' : 'status-missing'}</div>
    </div>
  );
}

describe('RobotContext', () => {
  const position: RobotPosition = {
    x: 1,
    y: 2,
    z: 3,
    rotationX: 0,
    rotationY: 0,
    rotationZ: 0,
    timestamp: new Date().toISOString(),
  };

  const status: RobotStatus = {
    isConnected: true,
    state: 'Idle',
    temperature: 20,
    errorCode: 0,
    errorMessage: undefined,
    loadPercentage: 5,
    timestamp: new Date().toISOString(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('loads initial data and connects to SignalR', async () => {
    vi.mocked(apiClient.getPosition).mockResolvedValue(position);
    vi.mocked(apiClient.getStatus).mockResolvedValue(status);
    vi.mocked(signalRService.connect).mockResolvedValue(true);

    render(<TestConsumer />);

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('done');
    });

    expect(screen.getByTestId('connection')).toHaveTextContent('Connected');
    expect(screen.getByTestId('position')).toHaveTextContent('position-loaded');
    expect(screen.getByTestId('status')).toHaveTextContent('status-loaded');
    expect(signalRService.onPositionUpdate).toHaveBeenCalledTimes(1);
    expect(signalRService.onStatusUpdate).toHaveBeenCalledTimes(1);
  });

  it('captures API failures during initial load', async () => {
    vi.mocked(apiClient.getPosition).mockRejectedValue(new Error('503 Service Unavailable'));
    vi.mocked(apiClient.getStatus).mockRejectedValue(new Error('503 Service Unavailable'));
    vi.mocked(signalRService.connect).mockResolvedValue(true);

    render(<TestConsumer />);

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('done');
    });

    const errorText = screen.getByTestId('error').textContent ?? '';
    expect(errorText).toMatch(/Failed to fetch (position|status)/);
  });
});
