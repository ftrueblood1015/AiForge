import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';
const API_KEY = import.meta.env.VITE_API_KEY || '';

const client = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
    'X-Api-Key': API_KEY,
  },
});

// Request interceptor for logging
client.interceptors.request.use(
  (config) => {
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      const { status, data } = error.response;

      if (status === 401) {
        console.error('[API] Unauthorized - check API key');
      } else if (status === 429) {
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
