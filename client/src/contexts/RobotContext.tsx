import { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import type { RobotPosition, RobotStatus, ConnectionStatus } from '../types';
import { signalRService } from '../services/signalRService';
import { apiClient } from '../services/apiClient';

interface RobotContextType {
  position: RobotPosition | null;
  status: RobotStatus | null;
  connectionStatus: ConnectionStatus;
  isLoading: boolean;
  error: string | null;
  refreshPosition: () => Promise<void>;
  refreshStatus: () => Promise<void>;
}

const RobotContext = createContext<RobotContextType | undefined>(undefined);

export function RobotProvider({ children }: { children: ReactNode }) {
  const [position, setPosition] = useState<RobotPosition | null>(null);
  const [status, setStatus] = useState<RobotStatus | null>(null);
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('Disconnected');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refreshPosition = useCallback(async () => {
    try {
      const pos = await apiClient.getPosition();
      console.log('Position received:', pos);
      setPosition(pos);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch position:', err);
      setError('Failed to fetch position');
    }
  }, []);

  const refreshStatus = useCallback(async () => {
    try {
      const stat = await apiClient.getStatus();
      console.log('Status received:', stat);
      setStatus(stat);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch status:', err);
      setError('Failed to fetch status');
    }
  }, []);

  useEffect(() => {
    const initializeConnection = async () => {
      setConnectionStatus('Connecting');
      setIsLoading(true);

      try {
        // Get initial data
        await Promise.all([refreshPosition(), refreshStatus()]);

        // Set up SignalR event listeners BEFORE connecting
        signalRService.onPositionUpdate((pos) => {
          setPosition(pos);
        });

        signalRService.onStatusUpdate((stat) => {
          setStatus(stat);
        });

        // Connect to SignalR
        const connected = await signalRService.connect();
        setConnectionStatus(connected ? 'Connected' : 'Failed');
      } catch (err) {
        console.error('Initialization error:', err);
        setConnectionStatus('Failed');
        setError('Failed to initialize connection');
      } finally {
        setIsLoading(false);
      }
    };

    initializeConnection();

    return () => {
      signalRService.disconnect();
    };
  }, [refreshPosition, refreshStatus]);

  return (
    <RobotContext.Provider
      value={{
        position,
        status,
        connectionStatus,
        isLoading,
        error,
        refreshPosition,
        refreshStatus,
      }}
    >
      {children}
    </RobotContext.Provider>
  );
}

export function useRobot() {
  const context = useContext(RobotContext);
  if (context === undefined) {
    throw new Error('useRobot must be used within a RobotProvider');
  }
  return context;
}
