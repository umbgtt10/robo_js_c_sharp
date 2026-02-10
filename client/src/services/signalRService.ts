import * as signalR from '@microsoft/signalr';
import type { RobotPosition, RobotStatus } from '../types';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelay = 1000;
  private maxReconnectDelay = 16000;

  async connect(): Promise<boolean> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return true;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/robot')
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
    }
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
    this.connection?.on('PositionUpdate', callback);
  }

  onStatusUpdate(callback: (status: RobotStatus) => void): void {
    this.connection?.on('StatusUpdate', callback);
  }

  offPositionUpdate(callback: (position: RobotPosition) => void): void {
    this.connection?.off('PositionUpdate', callback);
  }

  offStatusUpdate(callback: (status: RobotStatus) => void): void {
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
