import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  Skeleton,
  Alert,
  Button,
} from '@mui/material';
import {
  Description as HandoffIcon,
  Warning as WarningIcon,
  Celebration as MilestoneIcon,
  ExitToApp as SessionEndIcon,
  Storage as ContextDumpIcon,
} from '@mui/icons-material';
import { handoffsApi } from '../../api/handoffs';
import type { HandoffDocument, HandoffType } from '../../types';

interface TicketHandoffsProps {
  ticketId: string;
}

const typeConfig: Record<HandoffType, { icon: React.ReactNode; color: 'default' | 'primary' | 'warning' | 'info'; label: string }> = {
  SessionEnd: { icon: <SessionEndIcon fontSize="small" />, color: 'default', label: 'Session End' },
  Blocker: { icon: <WarningIcon fontSize="small" />, color: 'warning', label: 'Blocker' },
  Milestone: { icon: <MilestoneIcon fontSize="small" />, color: 'primary', label: 'Milestone' },
  ContextDump: { icon: <ContextDumpIcon fontSize="small" />, color: 'info', label: 'Context Dump' },
};

export default function TicketHandoffs({ ticketId }: TicketHandoffsProps) {
  const navigate = useNavigate();
  const [handoffs, setHandoffs] = useState<HandoffDocument[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadHandoffs();
  }, [ticketId]);

  const loadHandoffs = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await handoffsApi.getByTicket(ticketId);
      setHandoffs(data);
    } catch (err) {
      setError('Failed to load handoffs');
      console.error('Error loading handoffs:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box>
        {[1, 2].map((i) => (
          <Skeleton key={i} variant="rectangular" height={100} sx={{ mb: 2, borderRadius: 1 }} />
        ))}
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (handoffs.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <HandoffIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography color="text.secondary">No handoff documents for this ticket</Typography>
        <Typography variant="body2" color="text.secondary">
          Handoffs are created when work on a ticket is paused or completed
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="subtitle2" color="text.secondary">
          {handoffs.length} handoff{handoffs.length !== 1 ? 's' : ''}
        </Typography>
        <Button size="small" onClick={() => navigate('/handoffs')}>
          View All Handoffs
        </Button>
      </Box>

      {handoffs.map((handoff) => {
        const config = typeConfig[handoff.type];
        return (
          <Card
            key={handoff.id}
            sx={{
              mb: 2,
              opacity: handoff.isActive ? 1 : 0.7,
              border: handoff.type === 'Blocker' ? 2 : 0,
              borderColor: 'warning.main',
            }}
          >
            <CardActionArea onClick={() => navigate(`/handoffs/${handoff.id}`)}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                  <Box sx={{ color: `${config.color}.main`, mt: 0.25 }}>{config.icon}</Box>
                  <Box sx={{ flex: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {handoff.title}
                      </Typography>
                      <Chip label={config.label} size="small" color={config.color} />
                      {!handoff.isActive && (
                        <Chip label="Superseded" size="small" variant="outlined" />
                      )}
                    </Box>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(handoff.createdAt).toLocaleString()}
                    </Typography>
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{
                        mt: 1,
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                      }}
                    >
                      {handoff.summary}
                    </Typography>

                    {/* Context Summary */}
                    {handoff.structuredContext && (
                      <Box sx={{ display: 'flex', gap: 0.5, mt: 1, flexWrap: 'wrap' }}>
                        {handoff.structuredContext.blockers &&
                          handoff.structuredContext.blockers.length > 0 && (
                            <Chip
                              label={`${handoff.structuredContext.blockers.length} blocker(s)`}
                              size="small"
                              color="error"
                              variant="outlined"
                            />
                          )}
                        {handoff.structuredContext.nextSteps &&
                          handoff.structuredContext.nextSteps.length > 0 && (
                            <Chip
                              label={`${handoff.structuredContext.nextSteps.length} next step(s)`}
                              size="small"
                              color="info"
                              variant="outlined"
                            />
                          )}
                      </Box>
                    )}
                  </Box>
                </Box>
              </CardContent>
            </CardActionArea>
          </Card>
        );
      })}
    </Box>
  );
}
