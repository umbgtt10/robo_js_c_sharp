import axios, { AxiosInstance, AxiosError } from 'axios';
import type { RobotPosition, RobotStatus, MoveCommand, JogCommand, WorkEnvelope, ApiResponse } from '../types';

interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
}

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

    // REQUEST INTERCEPTOR - Add token to every request
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('authToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // RESPONSE INTERCEPTOR - Handle errors
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        // Token expired or invalid - redirect to login
        if (error.response?.status === 401) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        console.error('API Error:', error.message);
        return Promise.reject(error);
      }
    );
  }

  // AUTH METHODS
  async login(username: string, password: string): Promise<LoginResponse> {
    const response = await this.client.post<LoginResponse>('/auth/login', { username, password });

    // Store token and user info
    localStorage.setItem('authToken', response.data.token);
    localStorage.setItem('user', JSON.stringify({
      username: response.data.username,
      role: response.data.role
    }));

    return response.data;
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  }

  getCurrentUser(): { username: string; role: string } | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
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

