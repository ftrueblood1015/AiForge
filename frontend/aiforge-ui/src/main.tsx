import { StrictMode, useMemo, useSyncExternalStore } from 'react';
import { createRoot } from 'react-dom/client';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { BrowserRouter } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import { getTheme } from './theme';
import { useUiStore } from './stores/uiStore';
import App from './App';
import { ErrorBoundary } from './components/common';
import './index.css';

// Hook to detect system color scheme preference
function useSystemPrefersDark() {
  return useSyncExternalStore(
    (callback) => {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
      mediaQuery.addEventListener('change', callback);
      return () => mediaQuery.removeEventListener('change', callback);
    },
    () => window.matchMedia('(prefers-color-scheme: dark)').matches,
    () => false // SSR fallback
  );
}

function ThemedApp() {
  const themeMode = useUiStore((state) => state.themeMode);
  const systemPrefersDark = useSystemPrefersDark();

  const theme = useMemo(
    () => getTheme(themeMode, systemPrefersDark),
    [themeMode, systemPrefersDark]
  );

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider
        maxSnack={3}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        autoHideDuration={3000}
      >
        <App />
      </SnackbarProvider>
    </ThemeProvider>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <BrowserRouter>
        <ThemedApp />
      </BrowserRouter>
    </ErrorBoundary>
  </StrictMode>,
);
