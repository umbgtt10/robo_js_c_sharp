import { useState, useCallback } from 'react';
import { apiClient } from '../services/apiClient';
import type { MoveCommand, JogCommand } from '../types';

export function useRobotControl() {
  const [isExecuting, setIsExecuting] = useState(false);
  const [lastError, setLastError] = useState<string | null>(null);

  const moveToPosition = useCallback(async (command: MoveCommand): Promise<boolean> => {
    setIsExecuting(true);
    setLastError(null);

    try {
      const response = await apiClient.moveToPosition(command);
      if (response.error) {
        setLastError(response.error);
        return false;
      }
      return true;
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to execute move command';
      setLastError(message);
      return false;
    } finally {
      setIsExecuting(false);
    }
  }, []);

  const jog = useCallback(async (command: JogCommand): Promise<boolean> => {
    setIsExecuting(true);
    setLastError(null);

    try {
      const response = await apiClient.jog(command);
      if (response.error) {
        setLastError(response.error);
        return false;
      }
      return true;
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to execute jog command';
      setLastError(message);
      return false;
    } finally {
      setIsExecuting(false);
    }
  }, []);

  const emergencyStop = useCallback(async (): Promise<boolean> => {
    setLastError(null);

    try {
      const response = await apiClient.emergencyStop();
      if (response.error) {
        setLastError(response.error);
        return false;
      }
      return true;
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to execute emergency stop';
      setLastError(message);
      return false;
    }
  }, []);

  const home = useCallback(async (): Promise<boolean> => {
    setIsExecuting(true);
    setLastError(null);

    try {
      const response = await apiClient.home();
      if (response.error) {
        setLastError(response.error);
        return false;
      }
      return true;
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to execute homing sequence';
      setLastError(message);
      return false;
    } finally {
      setIsExecuting(false);
    }
  }, []);

  const resetError = useCallback(async (): Promise<boolean> => {
    setLastError(null);

    try {
      const response = await apiClient.resetError();
      if (response.error) {
        setLastError(response.error);
        return false;
      }
      return true;
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to reset error';
      setLastError(message);
      return false;
    }
  }, []);

  return {
    moveToPosition,
    jog,
    emergencyStop,
    home,
    resetError,
    isExecuting,
    lastError,
    clearError: () => setLastError(null),
  };
}
