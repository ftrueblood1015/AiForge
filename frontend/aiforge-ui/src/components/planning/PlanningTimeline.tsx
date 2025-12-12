import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Skeleton,
  Alert,
  Tabs,
  Tab,
  Badge,
} from '@mui/material';
import {
  Psychology as PlanningIcon,
  AccountTree as DecisionIcon,
  PlayArrow as ProgressIcon,
} from '@mui/icons-material';
import PlanningSessionCard from './PlanningSessionCard';
import ReasoningLogCard from './ReasoningLogCard';
import ProgressEntryCard from './ProgressEntryCard';
import { planningApi } from '../../api/planning';
import type { PlanningSession, ReasoningLog, ProgressEntry } from '../../types';

interface PlanningTimelineProps {
  ticketId: string;
}

type TimelineItem =
  | { type: 'session'; data: PlanningSession; timestamp: string }
  | { type: 'reasoning'; data: ReasoningLog; timestamp: string }
  | { type: 'progress'; data: ProgressEntry; timestamp: string };

export default function PlanningTimeline({ ticketId }: PlanningTimelineProps) {
  const [sessions, setSessions] = useState<PlanningSession[]>([]);
  const [reasoningLogs, setReasoningLogs] = useState<ReasoningLog[]>([]);
  const [progressEntries, setProgressEntries] = useState<ProgressEntry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);

  useEffect(() => {
    loadData();
  }, [ticketId]);

  const loadData = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await planningApi.getByTicket(ticketId);
      setSessions(data.planningSessions || []);
      setReasoningLogs(data.reasoningLogs || []);
      setProgressEntries(data.progressEntries || []);
    } catch (err) {
      setError('Failed to load planning data');
      console.error('Error loading planning data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Create unified timeline
  const timelineItems: TimelineItem[] = [
    ...sessions.map((s): TimelineItem => ({ type: 'session', data: s, timestamp: s.createdAt })),
    ...reasoningLogs.map((r): TimelineItem => ({ type: 'reasoning', data: r, timestamp: r.createdAt })),
    ...progressEntries.map((p): TimelineItem => ({ type: 'progress', data: p, timestamp: p.createdAt })),
  ].sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());

  if (isLoading) {
    return (
      <Box>
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} variant="rectangular" height={120} sx={{ mb: 2, borderRadius: 1 }} />
        ))}
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  const totalItems = sessions.length + reasoningLogs.length + progressEntries.length;

  if (totalItems === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <PlanningIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography color="text.secondary">
          No AI planning data yet
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Planning sessions, decisions, and progress will appear here
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Tabs for filtering */}
      <Tabs
        value={activeTab}
        onChange={(_, value) => setActiveTab(value)}
        sx={{ mb: 3, borderBottom: 1, borderColor: 'divider' }}
      >
        <Tab
          label={
            <Badge badgeContent={totalItems} color="primary" max={99}>
              <Box sx={{ pr: 2 }}>All</Box>
            </Badge>
          }
        />
        <Tab
          icon={<PlanningIcon />}
          iconPosition="start"
          label={
            <Badge badgeContent={sessions.length} color="info" max={99}>
              <Box sx={{ pr: 2 }}>Sessions</Box>
            </Badge>
          }
        />
        <Tab
          icon={<DecisionIcon />}
          iconPosition="start"
          label={
            <Badge badgeContent={reasoningLogs.length} color="secondary" max={99}>
              <Box sx={{ pr: 2 }}>Decisions</Box>
            </Badge>
          }
        />
        <Tab
          icon={<ProgressIcon />}
          iconPosition="start"
          label={
            <Badge badgeContent={progressEntries.length} color="success" max={99}>
              <Box sx={{ pr: 2 }}>Progress</Box>
            </Badge>
          }
        />
      </Tabs>

      {/* Timeline Content */}
      {activeTab === 0 && (
        // All items
        timelineItems.map((item, index) => {
          if (item.type === 'session') {
            return <PlanningSessionCard key={`session-${index}`} session={item.data} />;
          } else if (item.type === 'reasoning') {
            return <ReasoningLogCard key={`reasoning-${index}`} log={item.data} />;
          } else {
            return <ProgressEntryCard key={`progress-${index}`} entry={item.data} />;
          }
        })
      )}

      {activeTab === 1 && (
        // Sessions only
        sessions.length === 0 ? (
          <Typography color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
            No planning sessions
          </Typography>
        ) : (
          sessions
            .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            .map((session, index) => (
              <PlanningSessionCard key={index} session={session} />
            ))
        )
      )}

      {activeTab === 2 && (
        // Reasoning logs only
        reasoningLogs.length === 0 ? (
          <Typography color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
            No decisions logged
          </Typography>
        ) : (
          reasoningLogs
            .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            .map((log, index) => (
              <ReasoningLogCard key={index} log={log} />
            ))
        )
      )}

      {activeTab === 3 && (
        // Progress entries only
        progressEntries.length === 0 ? (
          <Typography color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
            No progress entries
          </Typography>
        ) : (
          progressEntries
            .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            .map((entry, index) => (
              <ProgressEntryCard key={index} entry={entry} />
            ))
        )
      )}
    </Box>
  );
}
