import { Chip } from '@mui/material';
import {
  PlayArrow as RunningIcon,
  Pause as PausedIcon,
  CheckCircle as CompletedIcon,
  Error as FailedIcon,
  Cancel as CancelledIcon,
  HourglassEmpty as PendingIcon,
} from '@mui/icons-material';
import type { ChainExecutionStatus } from '../../types';

interface ChainStatusChipProps {
  status: ChainExecutionStatus;
  size?: 'small' | 'medium';
}

const statusConfig: Record<ChainExecutionStatus, { color: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'; icon: React.ReactNode; label: string }> = {
  Pending: { color: 'default', icon: <PendingIcon fontSize="small" />, label: 'Pending' },
  Running: { color: 'primary', icon: <RunningIcon fontSize="small" />, label: 'Running' },
  Paused: { color: 'warning', icon: <PausedIcon fontSize="small" />, label: 'Paused' },
  Completed: { color: 'success', icon: <CompletedIcon fontSize="small" />, label: 'Completed' },
  Failed: { color: 'error', icon: <FailedIcon fontSize="small" />, label: 'Failed' },
  Cancelled: { color: 'default', icon: <CancelledIcon fontSize="small" />, label: 'Cancelled' },
};

export default function ChainStatusChip({ status, size = 'small' }: ChainStatusChipProps) {
  const config = statusConfig[status];

  return (
    <Chip
      icon={config.icon}
      label={config.label}
      color={config.color}
      size={size}
      sx={{ fontWeight: 500 }}
    />
  );
}
