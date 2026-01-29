import {
  Box,
  Typography,
  Paper,
  Chip,
  Tooltip,
  Divider,
} from '@mui/material';
import {
  CheckCircle as SuccessIcon,
  Error as FailureIcon,
  HourglassEmpty as PendingIcon,
  SkipNext as SkippedIcon,
  ArrowForward as TransitionIcon,
} from '@mui/icons-material';
import type { SkillChainLinkExecution, LinkExecutionOutcome } from '../../types';
import TransitionTypeChip from './TransitionTypeChip';

interface ExecutionTimelineProps {
  linkExecutions: SkillChainLinkExecution[];
}

const outcomeConfig: Record<LinkExecutionOutcome, { color: 'default' | 'primary' | 'success' | 'error' | 'warning'; icon: React.ReactNode }> = {
  Pending: { color: 'default', icon: <PendingIcon fontSize="small" /> },
  Success: { color: 'success', icon: <SuccessIcon fontSize="small" /> },
  Failure: { color: 'error', icon: <FailureIcon fontSize="small" /> },
  Skipped: { color: 'warning', icon: <SkippedIcon fontSize="small" /> },
};

function formatDuration(start: string, end: string | null): string {
  if (!end) return 'In progress...';
  const startDate = new Date(start);
  const endDate = new Date(end);
  const durationMs = endDate.getTime() - startDate.getTime();

  if (durationMs < 1000) return `${durationMs}ms`;
  if (durationMs < 60000) return `${Math.round(durationMs / 1000)}s`;
  return `${Math.round(durationMs / 60000)}m`;
}

function formatTime(dateString: string): string {
  return new Date(dateString).toLocaleTimeString();
}

export default function ExecutionTimeline({ linkExecutions }: ExecutionTimelineProps) {
  if (linkExecutions.length === 0) {
    return (
      <Box sx={{ p: 2, textAlign: 'center' }}>
        <Typography color="text.secondary">No executions yet</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ position: 'relative' }}>
      {/* Timeline Line */}
      <Box
        sx={{
          position: 'absolute',
          left: 18,
          top: 0,
          bottom: 0,
          width: 2,
          bgcolor: 'divider',
        }}
      />

      {/* Timeline Items */}
      {linkExecutions.map((exec, index) => {
        const config = outcomeConfig[exec.outcome];
        const isLast = index === linkExecutions.length - 1;

        return (
          <Box key={exec.id} sx={{ position: 'relative', pb: isLast ? 0 : 2 }}>
            {/* Timeline Dot */}
            <Box
              sx={{
                position: 'absolute',
                left: 9,
                width: 20,
                height: 20,
                borderRadius: '50%',
                bgcolor: 'background.paper',
                border: 2,
                borderColor: `${config.color}.main`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                zIndex: 1,
              }}
            >
              {config.icon}
            </Box>

            {/* Content */}
            <Paper
              variant="outlined"
              sx={{ ml: 5, p: 1.5 }}
            >
              {/* Header */}
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="subtitle2" fontWeight={600}>
                    {exec.linkName || `Position ${exec.linkPosition}`}
                  </Typography>
                  {exec.attemptNumber > 1 && (
                    <Chip
                      label={`Attempt ${exec.attemptNumber}`}
                      size="small"
                      color="warning"
                      variant="outlined"
                      sx={{ height: 20, fontSize: '0.65rem' }}
                    />
                  )}
                </Box>
                <Chip
                  label={exec.outcome}
                  size="small"
                  color={config.color}
                  sx={{ fontWeight: 500 }}
                />
              </Box>

              {/* Timing */}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                <Typography variant="caption" color="text.secondary">
                  Started: {formatTime(exec.startedAt)}
                </Typography>
                {exec.completedAt && (
                  <Typography variant="caption" color="text.secondary">
                    Duration: {formatDuration(exec.startedAt, exec.completedAt)}
                  </Typography>
                )}
                {exec.executedBy && (
                  <Typography variant="caption" color="text.disabled">
                    By: {exec.executedBy}
                  </Typography>
                )}
              </Box>

              {/* Transition Taken */}
              {exec.transitionTaken && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <TransitionIcon fontSize="small" color="action" />
                  <Typography variant="caption" color="text.secondary">
                    Transition:
                  </Typography>
                  <TransitionTypeChip transition={exec.transitionTaken} />
                </Box>
              )}

              {/* Error Details */}
              {exec.errorDetails && (
                <Box sx={{ mt: 1 }}>
                  <Divider sx={{ my: 1 }} />
                  <Typography variant="caption" color="error" component="div">
                    Error: {exec.errorDetails}
                  </Typography>
                </Box>
              )}

              {/* Output Preview */}
              {exec.output && (
                <Box sx={{ mt: 1 }}>
                  <Divider sx={{ my: 1 }} />
                  <Tooltip title="Click to expand">
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{
                        display: 'block',
                        maxHeight: 60,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        fontFamily: 'monospace',
                        bgcolor: 'grey.50',
                        p: 1,
                        borderRadius: 1,
                        cursor: 'pointer',
                      }}
                    >
                      {exec.output}
                    </Typography>
                  </Tooltip>
                </Box>
              )}
            </Paper>
          </Box>
        );
      })}
    </Box>
  );
}
