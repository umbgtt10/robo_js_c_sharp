// TypeScript type definitions for the application

export interface RobotPosition {
  x: number;
  y: number;
  z: number;
  rotationX: number;
  rotationY: number;
  rotationZ: number;
  timestamp: string;
}

export interface RobotStatus {
  isConnected: boolean;
  state: string;
  temperature: number;
  errorCode: number;
  errorMessage?: string;
  loadPercentage: number;
  timestamp: string;
}

export interface MoveCommand {
  x: number;
  y: number;
  z: number;
  rotationX?: number;
  rotationY?: number;
  rotationZ?: number;
}

export interface JogCommand {
  deltaX: number;
  deltaY: number;
  deltaZ: number;
}

export interface WorkEnvelope {
  xMin: number;
  xMax: number;
  yMin: number;
  yMax: number;
  zMin: number;
  zMax: number;
}

export interface ApiResponse<T = unknown> {
  data?: T;
  error?: string;
  message?: string;
}

export enum RobotState {
  Disconnected = 'Disconnected',
  Idle = 'Idle',
  Moving = 'Moving',
  EmergencyStopped = 'EmergencyStopped',
  Error = 'Error',
  Homing = 'Homing',
}

export enum ConnectionStatus {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
  Failed = 'Failed',
}
