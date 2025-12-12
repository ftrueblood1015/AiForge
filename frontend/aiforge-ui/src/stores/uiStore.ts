import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { ThemeMode } from '../theme';

interface UiState {
  sidebarOpen: boolean;
  themeMode: ThemeMode;

  // Actions
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  setThemeMode: (mode: ThemeMode) => void;
}

export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      sidebarOpen: true,
      themeMode: 'system',

      toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
      setSidebarOpen: (open: boolean) => set({ sidebarOpen: open }),
      setThemeMode: (mode: ThemeMode) => set({ themeMode: mode }),
    }),
    {
      name: 'aiforge-ui-settings',
      partialize: (state) => ({ themeMode: state.themeMode }),
    }
  )
);
