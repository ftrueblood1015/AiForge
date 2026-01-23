import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Typography,
  Box,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Lock as LockIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';

interface CheckoutDialogProps {
  open: boolean;
  onClose: () => void;
  onCheckout: (durationMinutes?: number) => Promise<void>;
  queueName: string;
  isConflict?: boolean;
  conflictInfo?: {
    checkedOutBy: string;
    expiresAt?: string;
  };
}

type DurationOption = '30' | '60' | '120' | 'none';

const durationOptions: { value: DurationOption; label: string; minutes?: number }[] = [
  { value: '30', label: '30 minutes', minutes: 30 },
  { value: '60', label: '1 hour', minutes: 60 },
  { value: '120', label: '2 hours', minutes: 120 },
  { value: 'none', label: 'No time limit' },
];

export default function CheckoutDialog({
  open,
  onClose,
  onCheckout,
  queueName,
  isConflict = false,
  conflictInfo,
}: CheckoutDialogProps) {
  const [duration, setDuration] = useState<DurationOption>('60');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCheckout = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const selectedOption = durationOptions.find((o) => o.value === duration);
      await onCheckout(selectedOption?.minutes);
      onClose();
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    if (!isLoading) {
      setError(null);
      setDuration('60');
      onClose();
    }
  };

  if (isConflict && conflictInfo) {
    return (
      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningIcon color="warning" />
          Queue Already Checked Out
        </DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This queue is currently checked out by another user.
          </Alert>
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">
              Checked out by
            </Typography>
            <Typography variant="body1">{conflictInfo.checkedOutBy}</Typography>
          </Box>
          {conflictInfo.expiresAt && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary">
                Expires at
              </Typography>
              <Typography variant="body1">
                {new Date(conflictInfo.expiresAt).toLocaleString()}
              </Typography>
            </Box>
          )}
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Please wait for the checkout to expire or ask {conflictInfo.checkedOutBy} to release it.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Close</Button>
        </DialogActions>
      </Dialog>
    );
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <LockIcon color="primary" />
        Checkout Queue
      </DialogTitle>
      <DialogContent>
        <Typography variant="body1" sx={{ mb: 3 }}>
          You're about to checkout <strong>{queueName}</strong>. This lets others know you're
          actively working on this queue and prevents conflicting changes.
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <FormControl component="fieldset">
          <FormLabel component="legend">Checkout Duration</FormLabel>
          <RadioGroup
            value={duration}
            onChange={(e) => setDuration(e.target.value as DurationOption)}
          >
            {durationOptions.map((option) => (
              <FormControlLabel
                key={option.value}
                value={option.value}
                control={<Radio />}
                label={option.label}
                disabled={isLoading}
              />
            ))}
          </RadioGroup>
        </FormControl>

        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
          You can release the checkout early at any time. If you choose a time limit, the checkout
          will automatically expire after the selected duration.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleCheckout}
          disabled={isLoading}
          startIcon={isLoading ? <CircularProgress size={20} /> : <LockIcon />}
        >
          {isLoading ? 'Checking out...' : 'Checkout'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
