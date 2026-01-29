import {
  Card,
  CardContent,
  Typography,
  Box,
  IconButton,
  Tooltip,
  Chip,
  LinearProgress,
} from '@mui/material';
import {
  MoreVert as MoreIcon,
  Warning as InterventionIcon,
  ConfirmationNumber as TicketIcon,
} from '@mui/icons-material';
import type { SkillChainExecutionSummary } from '../../types';
import ChainStatusChip from './ChainStatusChip';

interface ExecutionCardProps {
  execution: SkillChainExecutionSummary;
  onClick?: (execution: SkillChainExecutionSummary) => void;
  onMenuClick?: (event: React.MouseEvent, execution: SkillChainExecutionSummary) => void;
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleString();
}

function formatDuration(start: string, end: string | null): string {
  if (!end) {
    const startDate = new Date(start);
    const now = new Date();
    const durationMs = now.getTime() - startDate.getTime();
    const minutes = Math.floor(durationMs / 60000);
    if (minutes < 1) return 'Just started';
    if (minutes < 60) return `${minutes}m running`;
    return `${Math.floor(minutes / 60)}h ${minutes % 60}m running`;
  }

  const startDate = new Date(start);
  const endDate = new Date(end);
  const durationMs = endDate.getTime() - startDate.getTime();
  const minutes = Math.floor(durationMs / 60000);

  if (minutes < 1) return '< 1m';
  if (minutes < 60) return `${minutes}m`;
  return `${Math.floor(minutes / 60)}h ${minutes % 60}m`;
}

export default function ExecutionCard({
  execution,
  onClick,
  onMenuClick,
}: ExecutionCardProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(execution);
    }
  };

  const handleMenuClick = (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onMenuClick) {
      onMenuClick(event, execution);
    }
  };

  const isRunning = execution.status === 'Running';
  const needsIntervention = execution.requiresHumanIntervention;

  return (
    <Card
      sx={{
        cursor: onClick ? 'pointer' : 'default',
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': onClick
          ? {
              boxShadow: 3,
              transform: 'translateY(-2px)',
            }
          : {},
        borderLeft: needsIntervention ? '4px solid' : 'none',
        borderLeftColor: needsIntervention ? 'warning.main' : 'transparent',
      }}
      onClick={handleClick}
    >
      {isRunning && <LinearProgress sx={{ height: 2 }} />}
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ChainStatusChip status={execution.status} />
            {needsIntervention && (
              <Tooltip title="Requires human intervention">
                <InterventionIcon color="warning" fontSize="small" />
              </Tooltip>
            )}
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            {onMenuClick && (
              <Tooltip title="More options">
                <IconButton size="small" onClick={handleMenuClick}>
                  <MoreIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        </Box>

        {/* Chain Name */}
        <Typography
          variant="body1"
          fontWeight={500}
          sx={{
            mb: 0.5,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {execution.chainName || 'Unknown Chain'}
        </Typography>

        {/* Current Link */}
        {execution.currentLinkName && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            Current: <strong>{execution.currentLinkName}</strong>
          </Typography>
        )}

        {/* Stats Row */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1, flexWrap: 'wrap' }}>
          {execution.ticketKey && (
            <Tooltip title="Associated ticket">
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <TicketIcon fontSize="small" color="action" />
                <Typography variant="caption" color="text.secondary">
                  {execution.ticketKey}
                </Typography>
              </Box>
            </Tooltip>
          )}
          {execution.totalFailureCount > 0 && (
            <Chip
              label={`${execution.totalFailureCount} failure${execution.totalFailureCount > 1 ? 's' : ''}`}
              size="small"
              color="error"
              variant="outlined"
              sx={{ height: 20, fontSize: '0.65rem' }}
            />
          )}
        </Box>

        {/* Footer - Timing */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 1 }}>
          <Typography variant="caption" color="text.disabled">
            Started: {formatDate(execution.startedAt)}
          </Typography>
          <Typography variant="caption" color="text.disabled">
            {formatDuration(execution.startedAt, execution.completedAt)}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
}
