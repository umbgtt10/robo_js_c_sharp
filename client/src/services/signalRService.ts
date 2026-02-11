import * as signalR from '@microsoft/signalr';
import type { RobotPosition, RobotStatus } from '../types';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelay = 1000;
  private maxReconnectDelay = 16000;
  private positionHandlers = new Set<(position: RobotPosition) => void>();
  private statusHandlers = new Set<(status: RobotStatus) => void>();
  private connectingPromise: Promise<boolean> | null = null;

  async connect(): Promise<boolean> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return true;
    }

    if (this.connection?.state === signalR.HubConnectionState.Connecting && this.connectingPromise) {
      return this.connectingPromise;
    }

    if (this.connectingPromise) {
      return this.connectingPromise;
    }

    this.connectingPromise = (async () => {
      // Use absolute URL to backend, Vite proxy doesn't work reliably for WebSockets
      const hubUrl = import.meta.env.DEV
        ? 'https://localhost:5001/hubs/robot'
        : '/hubs/robot';

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          skipNegotiation: false,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            const delay = Math.min(
              this.reconnectDelay * Math.pow(2, retryContext.previousRetryCount),
              this.maxReconnectDelay
            );
            console.log(`SignalR reconnecting in ${delay}ms (attempt ${retryContext.previousRetryCount + 1})`);
            return delay;
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.attachUpdateHandlers();

      this.setupEventHandlers();

      try {
        await this.connection.start();
        console.log('SignalR connected');
        this.reconnectAttempts = 0;
        return true;
      } catch (error) {
        console.error('SignalR connection error:', error);
        this.scheduleReconnect();
        return false;
      } finally {
        this.connectingPromise = null;
      }
    })();

    return this.connectingPromise;
  }

  private setupEventHandlers(): void {
    if (!this.connection) return;

    this.connection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.scheduleReconnect();
    });

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected', connectionId);
      this.reconnectAttempts = 0;
    });
  }

  private attachUpdateHandlers(): void {
    if (!this.connection) return;

    this.positionHandlers.forEach((handler) => {
      this.connection?.on('PositionUpdate', handler);
    });

    this.statusHandlers.forEach((handler) => {
      this.connection?.on('StatusUpdate', handler);
    });
  }

  private scheduleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Max reconnection attempts reached');
      return;
    }

    const delay = Math.min(
      this.reconnectDelay * Math.pow(2, this.reconnectAttempts),
      this.maxReconnectDelay
    );

    this.reconnectAttempts++;
    console.log(`Scheduling reconnection attempt ${this.reconnectAttempts} in ${delay}ms`);

    setTimeout(() => {
      this.connect();
    }, delay);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  onPositionUpdate(callback: (position: RobotPosition) => void): void {
    this.positionHandlers.add(callback);
    this.connection?.on('PositionUpdate', callback);
  }

  onStatusUpdate(callback: (status: RobotStatus) => void): void {
    this.statusHandlers.add(callback);
    this.connection?.on('StatusUpdate', callback);
  }

  offPositionUpdate(callback: (position: RobotPosition) => void): void {
    this.positionHandlers.delete(callback);
    this.connection?.off('PositionUpdate', callback);
  }

  offStatusUpdate(callback: (status: RobotStatus) => void): void {
    this.statusHandlers.delete(callback);
    this.connection?.off('StatusUpdate', callback);
  }

  get connectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const signalRService = new SignalRService();
export default signalRService;
