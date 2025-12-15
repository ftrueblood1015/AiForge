import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Skeleton,
  Alert,
} from '@mui/material';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
} from '@mui/lab';
import {
  Assessment as EstimationIcon,
  Edit as RevisionIcon,
  CheckCircle as ActualIcon,
} from '@mui/icons-material';
import { estimationApi } from '../../api/estimation';
import EstimationCard from './EstimationCard';
import type { EffortEstimation } from '../../types';

interface EstimationHistoryTimelineProps {
  ticketId: string;
}

export default function EstimationHistoryTimeline({ ticketId }: EstimationHistoryTimelineProps) {
  const [estimations, setEstimations] = useState<EffortEstimation[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadHistory();
  }, [ticketId]);

  const loadHistory = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await estimationApi.getHistory(ticketId);
      // Sort by version descending (newest first)
      const sorted = [...data.estimations].sort((a, b) => b.version - a.version);
      setEstimations(sorted);
    } catch (err) {
      setError('Failed to load estimation history');
      console.error('Error loading estimation history:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box>
        {[1, 2].map((i) => (
          <Skeleton key={i} variant="rectangular" height={150} sx={{ mb: 2, borderRadius: 1 }} />
        ))}
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (estimations.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <EstimationIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography color="text.secondary">No estimation history</Typography>
        <Typography variant="body2" color="text.secondary">
          Estimates and revisions will appear here
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Estimation History ({estimations.length} version{estimations.length !== 1 ? 's' : ''})
      </Typography>

      <Timeline position="right" sx={{ p: 0, m: 0 }}>
        {estimations.map((estimation, index) => {
          const isFirst = index === 0;
          const isLast = index === estimations.length - 1;
          const hasActual = estimation.actualEffort !== null;
          const isRevision = estimation.version > 1;

          // Determine icon and color
          let icon = <EstimationIcon />;
          let dotColor: 'primary' | 'secondary' | 'success' = 'primary';

          if (hasActual) {
            icon = <ActualIcon />;
            dotColor = 'success';
          } else if (isRevision) {
            icon = <RevisionIcon />;
            dotColor = 'secondary';
          }

          return (
            <TimelineItem key={estimation.id}>
              <TimelineOppositeContent
                sx={{ flex: 0.2, minWidth: 100 }}
                color="text.secondary"
                variant="body2"
              >
                {new Date(estimation.createdAt).toLocaleDateString()}
              </TimelineOppositeContent>
              <TimelineSeparator>
                <TimelineDot color={dotColor}>{icon}</TimelineDot>
                {!isLast && <TimelineConnector />}
              </TimelineSeparator>
              <TimelineContent sx={{ flex: 1 }}>
                <EstimationCard estimation={estimation} showVariance={isFirst} />
              </TimelineContent>
            </TimelineItem>
          );
        })}
      </Timeline>
    </Box>
  );
}
