import {
  Alert,
  AlertTitle,
  Box,
  Button,
  Typography,
} from '@mui/material';
import {
  Warning as WarningIcon,
  Replay as RetryIcon,
  SkipNext as SkipIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material';

interface InterventionBannerProps {
  reason: string | null;
  failureCount: number;
  onRetry?: () => void;
  onSkip?: () => void;
  onCancel?: () => void;
}

export default function InterventionBanner({
  reason,
  failureCount,
  onRetry,
  onSkip,
  onCancel,
}: InterventionBannerProps) {
  return (
    <Alert
      severity="warning"
      icon={<WarningIcon fontSize="large" />}
      sx={{
        mb: 2,
        '& .MuiAlert-message': {
          width: '100%',
        },
      }}
    >
      <AlertTitle sx={{ fontWeight: 600, fontSize: '1rem' }}>
        Human Intervention Required
      </AlertTitle>

      <Typography variant="body2" sx={{ mb: 2 }}>
        {reason || 'This execution requires human review before it can continue.'}
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <Typography variant="caption" color="text.secondary">
          Total failures: <strong>{failureCount}</strong>
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        {onRetry && (
          <Button
            variant="contained"
            size="small"
            color="primary"
            startIcon={<RetryIcon />}
            onClick={onRetry}
          >
            Retry Current Link
          </Button>
        )}
        {onSkip && (
          <Button
            variant="outlined"
            size="small"
            color="secondary"
            startIcon={<SkipIcon />}
            onClick={onSkip}
          >
            Skip to Different Link
          </Button>
        )}
        {onCancel && (
          <Button
            variant="outlined"
            size="small"
            color="error"
            startIcon={<CancelIcon />}
            onClick={onCancel}
          >
            Cancel Execution
          </Button>
        )}
      </Box>
    </Alert>
  );
}
