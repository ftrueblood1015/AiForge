import { Chip } from '@mui/material';
import type { WorkQueueItemStatus } from '../../types';

const statusColors: Record<WorkQueueItemStatus, 'default' | 'primary' | 'success' | 'warning' | 'error'> = {
  Pending: 'default',
  InProgress: 'primary',
  Completed: 'success',
  Skipped: 'warning',
  Blocked: 'error',
};

const statusLabels: Record<WorkQueueItemStatus, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Skipped: 'Skipped',
  Blocked: 'Blocked',
};

interface QueueItemStatusChipProps {
  status: WorkQueueItemStatus;
  size?: 'small' | 'medium';
  onClick?: () => void;
}

export default function QueueItemStatusChip({ status, size = 'small', onClick }: QueueItemStatusChipProps) {
  return (
    <Chip
      label={statusLabels[status]}
      size={size}
      color={statusColors[status]}
      onClick={onClick}
      sx={onClick ? { cursor: 'pointer' } : undefined}
    />
  );
}
