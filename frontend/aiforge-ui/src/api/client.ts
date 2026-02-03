import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore, getAccessToken, isTokenExpired } from '../stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';
const API_KEY = import.meta.env.VITE_API_KEY || '';

const client = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Flag to prevent multiple refresh attempts
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: Error) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

// Request interceptor for adding auth token
client.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);

    // Get current access token
    const accessToken = getAccessToken();

    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    } else if (API_KEY) {
      // Fallback to API key if no JWT token
      config.headers['X-Api-Key'] = API_KEY;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling and token refresh
client.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Handle 401 errors with token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      const authStore = useAuthStore.getState();

      // If we have a refresh token and token is expired, try to refresh
      if (authStore.tokens?.refreshToken && isTokenExpired()) {
        if (isRefreshing) {
          // Wait for the refresh to complete
          return new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject });
          })
            .then((token) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              return client(originalRequest);
            })
            .catch((err) => Promise.reject(err));
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
          const refreshed = await authStore.refreshToken();
          if (refreshed) {
            const newToken = useAuthStore.getState().tokens?.accessToken;
            processQueue(null, newToken);
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return client(originalRequest);
          } else {
            processQueue(new Error('Refresh failed'), null);
            // Redirect to login
            window.location.href = '/login';
            return Promise.reject(error);
          }
        } catch (refreshError) {
          processQueue(refreshError as Error, null);
          window.location.href = '/login';
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      console.error('[API] Unauthorized - authentication required');
    } else if (error.response) {
      const { status, data } = error.response;

      if (status === 429) {
        console.error('[API] Rate limit exceeded');
      } else if (status === 404) {
        console.error('[API] Resource not found');
      } else {
        console.error(`[API] Error ${status}:`, data);
      }
    } else if (error.request) {
      console.error('[API] No response received:', error.message);
    } else {
      console.error('[API] Request error:', error.message);
    }

    return Promise.reject(error);
  }
);

export default client;
