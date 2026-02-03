import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User, TokenPair, LoginRequest, RegisterRequest } from '../types';
import { authApi } from '../api/auth';

interface AuthState {
  user: User | null;
  tokens: TokenPair | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  clearError: () => void;
  setTokens: (tokens: TokenPair) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      tokens: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      login: async (request: LoginRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.login(request);
          set({
            user: response.user,
            tokens: response.tokens,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error: unknown) {
          const message = error instanceof Error ? error.message :
            (error as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Login failed';
          set({ error: message, isLoading: false });
          throw error;
        }
      },

      register: async (request: RegisterRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.register(request);
          set({
            user: response.user,
            tokens: response.tokens,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error: unknown) {
          const message = error instanceof Error ? error.message :
            (error as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Registration failed';
          set({ error: message, isLoading: false });
          throw error;
        }
      },

      logout: async () => {
        const { tokens } = get();
        try {
          if (tokens?.accessToken) {
            await authApi.logout(tokens.accessToken);
          }
        } catch {
          // Ignore errors during logout
        }
        set({
          user: null,
          tokens: null,
          isAuthenticated: false,
          error: null,
        });
      },

      refreshToken: async () => {
        const { tokens } = get();
        if (!tokens?.refreshToken) {
          return false;
        }

        try {
          const response = await authApi.refreshToken({ refreshToken: tokens.refreshToken });
          set({
            user: response.user,
            tokens: response.tokens,
            isAuthenticated: true,
          });
          return true;
        } catch {
          // Refresh failed, logout
          set({
            user: null,
            tokens: null,
            isAuthenticated: false,
          });
          return false;
        }
      },

      clearError: () => set({ error: null }),

      setTokens: (tokens: TokenPair) => set({ tokens }),
    }),
    {
      name: 'aiforge-auth',
      partialize: (state) => ({
        user: state.user,
        tokens: state.tokens,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

// Helper to get access token for API calls
export const getAccessToken = (): string | null => {
  return useAuthStore.getState().tokens?.accessToken || null;
};

// Helper to check if token is expired
export const isTokenExpired = (): boolean => {
  const tokens = useAuthStore.getState().tokens;
  if (!tokens?.expiresAt) return true;
  return new Date(tokens.expiresAt) <= new Date();
};
