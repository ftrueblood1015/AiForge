import { Chip } from '@mui/material';
import {
  ArrowForward as NextIcon,
  Shortcut as GoToIcon,
  CheckCircle as CompleteIcon,
  Replay as RetryIcon,
  ReportProblem as EscalateIcon,
} from '@mui/icons-material';
import type { TransitionType } from '../../types';

interface TransitionTypeChipProps {
  transition: TransitionType;
  size?: 'small' | 'medium';
  variant?: 'filled' | 'outlined';
}

const transitionConfig: Record<TransitionType, { color: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'; icon: React.ReactNode; label: string }> = {
  NextLink: { color: 'info', icon: <NextIcon fontSize="small" />, label: 'Next Link' },
  GoToLink: { color: 'secondary', icon: <GoToIcon fontSize="small" />, label: 'Go To Link' },
  Complete: { color: 'success', icon: <CompleteIcon fontSize="small" />, label: 'Complete' },
  Retry: { color: 'warning', icon: <RetryIcon fontSize="small" />, label: 'Retry' },
  Escalate: { color: 'error', icon: <EscalateIcon fontSize="small" />, label: 'Escalate' },
};

export default function TransitionTypeChip({ transition, size = 'small', variant = 'outlined' }: TransitionTypeChipProps) {
  const config = transitionConfig[transition];

  return (
    <Chip
      icon={config.icon}
      label={config.label}
      color={config.color}
      size={size}
      variant={variant}
      sx={{ fontWeight: 500, fontSize: '0.75rem' }}
    />
  );
}
