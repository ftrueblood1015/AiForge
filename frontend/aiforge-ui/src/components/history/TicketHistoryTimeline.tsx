import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Skeleton,
  Alert,
  Chip,
} from '@mui/material';
import {
  Add as CreateIcon,
  SwapHoriz as ChangeIcon,
  Flag as StatusIcon,
  PriorityHigh as PriorityIcon,
  Label as TypeIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import { ticketsApi } from '../../api/tickets';
import type { TicketHistory, Ticket } from '../../types';

interface TicketHistoryTimelineProps {
  ticketId: string;
  ticket: Ticket;
}

const fieldConfig: Record<string, { icon: React.ReactNode; label: string; color: string }> = {
  Status: { icon: <StatusIcon fontSize="small" />, label: 'Status', color: 'primary.main' },
  Priority: { icon: <PriorityIcon fontSize="small" />, label: 'Priority', color: 'warning.main' },
  Type: { icon: <TypeIcon fontSize="small" />, label: 'Type', color: 'info.main' },
  Title: { icon: <EditIcon fontSize="small" />, label: 'Title', color: 'text.secondary' },
  Description: { icon: <EditIcon fontSize="small" />, label: 'Description', color: 'text.secondary' },
};

const statusColors: Record<string, 'default' | 'primary' | 'warning' | 'success'> = {
  ToDo: 'default',
  InProgress: 'primary',
  InReview: 'warning',
  Done: 'success',
};

const statusLabels: Record<string, string> = {
  ToDo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

export default function TicketHistoryTimeline({ ticketId, ticket }: TicketHistoryTimelineProps) {
  const [history, setHistory] = useState<TicketHistory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadHistory();
  }, [ticketId]);

  const loadHistory = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await ticketsApi.getHistory(ticketId);
      setHistory(data);
    } catch (err) {
      setError('Failed to load ticket history');
      console.error('Error loading history:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const formatValue = (field: string, value: string | null): React.ReactNode => {
    if (!value) return <Typography component="span" color="text.secondary" fontStyle="italic">none</Typography>;

    if (field === 'Status') {
      return (
        <Chip
          label={statusLabels[value] || value}
          size="small"
          color={statusColors[value] || 'default'}
        />
      );
    }

    return value;
  };

  if (isLoading) {
    return (
      <Box>
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} variant="rectangular" height={80} sx={{ mb: 2, borderRadius: 1 }} />
        ))}
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  // Combine creation event with history
  const timelineItems = [
    // Add ticket creation as the first event
    {
      id: 'created',
      type: 'created' as const,
      timestamp: ticket.createdAt,
    },
    // Add all history items
    ...history.map((h) => ({
      id: h.id,
      type: 'change' as const,
      timestamp: h.changedAt,
      data: h,
    })),
  ].sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());

  return (
    <Box>
      <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
        {timelineItems.length} event{timelineItems.length !== 1 ? 's' : ''}
      </Typography>

      <Box sx={{ position: 'relative', pl: 3 }}>
        {/* Timeline line */}
        <Box
          sx={{
            position: 'absolute',
            left: 11,
            top: 8,
            bottom: 8,
            width: 2,
            bgcolor: 'divider',
            borderRadius: 1,
          }}
        />

        {timelineItems.map((item, index) => {
          const isCreated = item.type === 'created';
          const config = isCreated
            ? { icon: <CreateIcon fontSize="small" />, label: 'Created', color: 'success.main' }
            : fieldConfig[item.data!.field] || { icon: <ChangeIcon fontSize="small" />, label: item.data!.field, color: 'text.secondary' };

          return (
            <Box
              key={item.id}
              sx={{
                position: 'relative',
                pb: index === timelineItems.length - 1 ? 0 : 3,
              }}
            >
              {/* Timeline dot */}
              <Box
                sx={{
                  position: 'absolute',
                  left: -19,
                  top: 4,
                  width: 24,
                  height: 24,
                  borderRadius: '50%',
                  bgcolor: 'background.paper',
                  border: 2,
                  borderColor: config.color,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: config.color,
                }}
              >
                {config.icon}
              </Box>

              {/* Content */}
              <Box
                sx={{
                  bgcolor: 'background.paper',
                  border: 1,
                  borderColor: 'divider',
                  borderRadius: 1,
                  p: 2,
                  ml: 1,
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                  <Typography variant="subtitle2" fontWeight={600}>
                    {isCreated ? 'Ticket Created' : `${config.label} Changed`}
                  </Typography>
                </Box>

                <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
                  {new Date(item.timestamp).toLocaleString()}
                </Typography>

                {!isCreated && item.data && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      {formatValue(item.data.field, item.data.oldValue)}
                    </Box>
                    <ChangeIcon fontSize="small" sx={{ color: 'text.secondary' }} />
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      {formatValue(item.data.field, item.data.newValue)}
                    </Box>
                  </Box>
                )}

                {!isCreated && item.data?.changedBy && (
                  <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                    by {item.data.changedBy}
                  </Typography>
                )}
              </Box>
            </Box>
          );
        })}
      </Box>

      {timelineItems.length === 1 && (
        <Box sx={{ textAlign: 'center', py: 2 }}>
          <Typography variant="body2" color="text.secondary">
            No changes have been made to this ticket yet.
          </Typography>
        </Box>
      )}
    </Box>
  );
}
