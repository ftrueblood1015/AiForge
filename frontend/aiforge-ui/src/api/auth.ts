import axios from 'axios';
import type { AuthResponse, LoginRequest, RegisterRequest, RefreshTokenRequest, User } from '../types';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

// Separate axios instance for auth (doesn't use interceptors that might cause loops)
const authClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authApi = {
  login: async (request: LoginRequest): Promise<AuthResponse> => {
    const response = await authClient.post<AuthResponse>('/api/auth/login', request);
    return response.data;
  },

  register: async (request: RegisterRequest): Promise<AuthResponse> => {
    const response = await authClient.post<AuthResponse>('/api/auth/register', request);
    return response.data;
  },

  refreshToken: async (request: RefreshTokenRequest): Promise<AuthResponse> => {
    const response = await authClient.post<AuthResponse>('/api/auth/refresh', request);
    return response.data;
  },

  logout: async (accessToken: string): Promise<void> => {
    await authClient.post('/api/auth/logout', {}, {
      headers: { Authorization: `Bearer ${accessToken}` }
    });
  },

  getCurrentUser: async (accessToken: string): Promise<User> => {
    const response = await authClient.get<User>('/api/auth/me', {
      headers: { Authorization: `Bearer ${accessToken}` }
    });
    return response.data;
  },
};
