import { Chip } from '@mui/material';
import type { WorkQueueStatus } from '../../types';

const statusColors: Record<WorkQueueStatus, 'success' | 'warning' | 'default' | 'error'> = {
  Active: 'success',
  Paused: 'warning',
  Completed: 'default',
  Archived: 'error',
};

const statusLabels: Record<WorkQueueStatus, string> = {
  Active: 'Active',
  Paused: 'Paused',
  Completed: 'Completed',
  Archived: 'Archived',
};

interface QueueStatusChipProps {
  status: WorkQueueStatus;
  size?: 'small' | 'medium';
}

export default function QueueStatusChip({ status, size = 'small' }: QueueStatusChipProps) {
  return (
    <Chip
      label={statusLabels[status]}
      size={size}
      color={statusColors[status]}
    />
  );
}
