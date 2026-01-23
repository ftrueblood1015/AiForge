import { Chip } from '@mui/material';
import type { AgentStatus } from '../../types';

const statusColors: Record<AgentStatus, 'default' | 'primary' | 'warning' | 'error' | 'success'> = {
  Idle: 'default',
  Working: 'primary',
  Paused: 'warning',
  Disabled: 'default',
  Error: 'error',
};

const statusLabels: Record<AgentStatus, string> = {
  Idle: 'Idle',
  Working: 'Working',
  Paused: 'Paused',
  Disabled: 'Disabled',
  Error: 'Error',
};

interface AgentStatusChipProps {
  status: AgentStatus;
  size?: 'small' | 'medium';
}

export default function AgentStatusChip({ status, size = 'small' }: AgentStatusChipProps) {
  return (
    <Chip
      label={statusLabels[status]}
      size={size}
      color={statusColors[status]}
      variant={status === 'Disabled' ? 'outlined' : 'filled'}
    />
  );
}
