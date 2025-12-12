import { useSnackbar, type VariantType } from 'notistack';
import { useCallback } from 'react';

interface NotificationOptions {
  variant?: VariantType;
  autoHideDuration?: number;
  persist?: boolean;
}

export function useNotification() {
  const { enqueueSnackbar, closeSnackbar } = useSnackbar();

  const notify = useCallback(
    (message: string, options?: NotificationOptions) => {
      return enqueueSnackbar(message, {
        variant: options?.variant || 'default',
        autoHideDuration: options?.autoHideDuration || 3000,
        persist: options?.persist || false,
      });
    },
    [enqueueSnackbar]
  );

  const success = useCallback(
    (message: string) => notify(message, { variant: 'success' }),
    [notify]
  );

  const error = useCallback(
    (message: string) => notify(message, { variant: 'error', autoHideDuration: 5000 }),
    [notify]
  );

  const warning = useCallback(
    (message: string) => notify(message, { variant: 'warning' }),
    [notify]
  );

  const info = useCallback(
    (message: string) => notify(message, { variant: 'info' }),
    [notify]
  );

  return {
    notify,
    success,
    error,
    warning,
    info,
    close: closeSnackbar,
  };
}
