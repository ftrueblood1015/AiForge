import { Chip } from '@mui/material';
import type { TicketStatus } from '../../types';

const statusColors: Record<TicketStatus, 'default' | 'primary' | 'warning' | 'success'> = {
  ToDo: 'default',
  InProgress: 'primary',
  InReview: 'warning',
  Done: 'success',
};

const statusLabels: Record<TicketStatus, string> = {
  ToDo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

interface StatusChipProps {
  status: TicketStatus;
  size?: 'small' | 'medium';
}

export default function StatusChip({ status, size = 'small' }: StatusChipProps) {
  return (
    <Chip
      label={statusLabels[status]}
      size={size}
      color={statusColors[status]}
    />
  );
}
