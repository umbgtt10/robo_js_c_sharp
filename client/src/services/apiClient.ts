import axios, { AxiosInstance, AxiosError } from 'axios';
import type { RobotPosition, RobotStatus, MoveCommand, JogCommand, WorkEnvelope, ApiResponse } from '../types';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: '/api',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        console.error('API Error:', error.message);
        return Promise.reject(error);
      }
    );
  }

  // Robot position operations
  async getPosition(): Promise<RobotPosition> {
    const response = await this.client.get<RobotPosition>('/robot/position');
    return response.data;
  }

  // Robot status operations
  async getStatus(): Promise<RobotStatus> {
    const response = await this.client.get<RobotStatus>('/robot/status');
    return response.data;
  }

  // Movement commands
  async moveToPosition(command: MoveCommand): Promise<ApiResponse> {
    const response = await this.client.post<ApiResponse>('/robot/move', command);
    return response.data;
  }

  async jog(command: JogCommand): Promise<ApiResponse> {
    const response = await this.client.post<ApiResponse>('/robot/jog', command);
    return response.data;
  }

  async emergencyStop(): Promise<ApiResponse> {
    const response = await this.client.post<ApiResponse>('/robot/emergency-stop');
    return response.data;
  }

  async home(): Promise<ApiResponse> {
    const response = await this.client.post<ApiResponse>('/robot/home');
    return response.data;
  }

  async resetError(): Promise<ApiResponse> {
    const response = await this.client.post<ApiResponse>('/robot/reset-error');
    return response.data;
  }

  // Configuration
  async getWorkEnvelope(): Promise<WorkEnvelope> {
    const response = await this.client.get<WorkEnvelope>('/configuration/work-envelope');
    return response.data;
  }

  async getConnectionSettings(): Promise<unknown> {
    const response = await this.client.get('/configuration/connection');
    return response.data;
  }

  // Health check
  async checkHealth(): Promise<{ status: string; timestamp: string }> {
    const response = await this.client.get('/health');
    return response.data;
  }
}

export const apiClient = new ApiClient();
export default apiClient;
