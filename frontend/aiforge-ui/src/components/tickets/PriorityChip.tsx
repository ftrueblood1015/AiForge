import { Chip } from '@mui/material';
import type { Priority } from '../../types';

const priorityColors: Record<Priority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

interface PriorityChipProps {
  priority: Priority;
  size?: 'small' | 'medium';
}

export default function PriorityChip({ priority, size = 'small' }: PriorityChipProps) {
  return (
    <Chip
      label={priority}
      size={size}
      color={priorityColors[priority]}
    />
  );
}
