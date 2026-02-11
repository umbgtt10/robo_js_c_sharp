import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useRobotControl } from '../useRobotControl';
import { apiClient } from '../../services/apiClient';

// Mock the apiClient
vi.mock('../../services/apiClient', () => ({
  apiClient: {
    jog: vi.fn(),
    moveToPosition: vi.fn(),
    emergencyStop: vi.fn(),
    home: vi.fn(),
    resetError: vi.fn(),
  },
}));

describe('useRobotControl', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('jog', () => {
    it('returns true on successful jog command', async () => {
      vi.mocked(apiClient.jog).mockResolvedValue({
        success: true,
        message: 'Movement completed',
      });

      const { result } = renderHook(() => useRobotControl());

      const success = await result.current.jog({
        deltaX: 10,
        deltaY: 0,
        deltaZ: 0,
      });

      expect(success).toBe(true);
      expect(apiClient.jog).toHaveBeenCalledWith({
        deltaX: 10,
        deltaY: 0,
        deltaZ: 0,
      });
    });

    it('returns false and sets error on failed jog command', async () => {
      vi.mocked(apiClient.jog).mockResolvedValue({
        error: 'Movement exceeds work envelope',
      });

      const { result } = renderHook(() => useRobotControl());

      const success = await result.current.jog({
        deltaX: 1000,
        deltaY: 0,
        deltaZ: 0,
      });

      expect(success).toBe(false);

      await waitFor(() => {
        expect(result.current.lastError).toBe('Movement exceeds work envelope');
      });
    });

    it('sets isExecuting to true during command execution', async () => {
      let resolvePromise: (value: any) => void;
      const promise = new Promise((resolve) => {
        resolvePromise = resolve;
      });

      vi.mocked(apiClient.jog).mockReturnValue(promise as any);

      const { result } = renderHook(() => useRobotControl());

      const jogPromise = result.current.jog({
        deltaX: 10,
        deltaY: 0,
        deltaZ: 0,
      });

      // Should be executing
      await waitFor(() => {
        expect(result.current.isExecuting).toBe(true);
      });

      // Resolve the API call
      resolvePromise!({ success: true });
      await jogPromise;

      // Should no longer be executing
      await waitFor(() => {
        expect(result.current.isExecuting).toBe(false);
      });
    });

    it('handles network errors gracefully', async () => {
      vi.mocked(apiClient.jog).mockRejectedValue(
        new Error('Network error')
      );

      const { result } = renderHook(() => useRobotControl());

      const success = await result.current.jog({
        deltaX: 10,
        deltaY: 0,
        deltaZ: 0,
      });

      expect(success).toBe(false);

      await waitFor(() => {
        expect(result.current.lastError).not.toBeNull();
        expect(result.current.lastError!).toContain('Network error');
      });
    });
  });

  describe('emergencyStop', () => {
    it('executes emergency stop immediately without setting isExecuting', async () => {
      vi.mocked(apiClient.emergencyStop).mockResolvedValue({
        success: true,
      });

      const { result } = renderHook(() => useRobotControl());

      const success = await result.current.emergencyStop();

      expect(success).toBe(true);
      expect(result.current.isExecuting).toBe(false); // Should not set to true
      expect(apiClient.emergencyStop).toHaveBeenCalled();
    });
  });

  describe('home', () => {
    it('returns true on successful home command', async () => {
      vi.mocked(apiClient.home).mockResolvedValue({
        success: true,
        message: 'Homing completed',
      });

      const { result } = renderHook(() => useRobotControl());

      const success = await result.current.home();

      expect(success).toBe(true);
      expect(apiClient.home).toHaveBeenCalled();
    });
  });

  describe('moveToPosition', () => {
    it('calls API with correct command', async () => {
      vi.mocked(apiClient.moveToPosition).mockResolvedValue({
        success: true,
      });

      const { result } = renderHook(() => useRobotControl());

      const command = {
        x: 100,
        y: 50,
        z: 200,
        rotationX: 0,
        rotationY: 0,
        rotationZ: 0,
      };

      await result.current.moveToPosition(command);

      expect(apiClient.moveToPosition).toHaveBeenCalledWith(command);
    });
  });
});
