import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Grid,
  Typography,
  Skeleton,
  Alert,
  LinearProgress,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Tooltip,
} from '@mui/material';
import {
  AccessTime as TimeIcon,
  Token as TokenIcon,
  Psychology as DecisionIcon,
  TrendingUp as ProgressIcon,
  Edit as FileIcon,
  Sync as HandoffIcon,
  PlayArrow as SessionIcon,
} from '@mui/icons-material';
import { analyticsApi } from '../../api/analytics';
import type { TicketSessionAnalytics, SessionMetrics } from '../../types';

interface TicketAnalyticsTabProps {
  ticketId: string;
}

interface StatCardProps {
  icon: React.ReactNode;
  label: string;
  value: string | number;
  subtitle?: string;
  color?: string;
}

function StatCard({ icon, label, value, subtitle, color = 'primary.main' }: StatCardProps) {
  return (
    <Card variant="outlined">
      <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: 40,
              height: 40,
              borderRadius: 1,
              bgcolor: `${color}15`,
              color: color,
            }}
          >
            {icon}
          </Box>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography variant="caption" color="text.secondary" display="block">
              {label}
            </Typography>
            <Typography variant="h6" fontWeight={600} noWrap>
              {value}
            </Typography>
            {subtitle && (
              <Typography variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}

function formatDuration(minutes: number): string {
  if (minutes < 60) return `${minutes}m`;
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
}

function formatTokens(tokens: number): string {
  if (tokens >= 1000000) return `${(tokens / 1000000).toFixed(1)}M`;
  if (tokens >= 1000) return `${(tokens / 1000).toFixed(1)}K`;
  return tokens.toString();
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function TicketAnalyticsTab({ ticketId }: TicketAnalyticsTabProps) {
  const [analytics, setAnalytics] = useState<TicketSessionAnalytics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await analyticsApi.getTicketSessionAnalytics(ticketId);
        setAnalytics(data);
      } catch (err) {
        console.error('Failed to load ticket analytics:', err);
        setError('Failed to load analytics data');
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, [ticketId]);

  if (loading) {
    return (
      <Box>
        <Grid container spacing={2} sx={{ mb: 3 }}>
          {[1, 2, 3, 4].map((i) => (
            <Grid size={{ xs: 6, sm: 3 }} key={i}>
              <Skeleton variant="rounded" height={90} />
            </Grid>
          ))}
        </Grid>
        <Skeleton variant="rounded" height={300} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (!analytics) {
    return <Alert severity="info">No analytics data available</Alert>;
  }

  const hasNoData =
    analytics.totalSessions === 0 &&
    analytics.totalDurationMinutes === 0 &&
    analytics.totalTokens === 0;

  if (hasNoData) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <SessionIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 2 }} />
        <Typography variant="h6" color="text.secondary" gutterBottom>
          No Analytics Yet
        </Typography>
        <Typography color="text.secondary">
          Analytics will appear here once work sessions are logged for this ticket.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard
            icon={<SessionIcon />}
            label="Sessions"
            value={analytics.totalSessions}
            subtitle={
              analytics.averageDurationMinutes
                ? `avg ${formatDuration(analytics.averageDurationMinutes)}`
                : undefined
            }
            color="primary.main"
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard
            icon={<TimeIcon />}
            label="Total Time"
            value={formatDuration(analytics.totalDurationMinutes)}
            color="info.main"
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard
            icon={<TokenIcon />}
            label="Tokens Used"
            value={formatTokens(analytics.totalTokens)}
            subtitle={
              analytics.totalInputTokens > 0
                ? `${formatTokens(analytics.totalInputTokens)} in / ${formatTokens(analytics.totalOutputTokens)} out`
                : undefined
            }
            color="warning.main"
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <StatCard
            icon={<DecisionIcon />}
            label="Decisions"
            value={analytics.totalDecisions}
            color="secondary.main"
          />
        </Grid>
      </Grid>

      {/* Additional Stats */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, sm: 4 }}>
          <StatCard
            icon={<ProgressIcon />}
            label="Progress Entries"
            value={analytics.totalProgressEntries}
            color="success.main"
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4 }}>
          <StatCard
            icon={<FileIcon />}
            label="Files Modified"
            value={analytics.totalFilesModified}
            color="error.main"
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4 }}>
          <StatCard
            icon={<HandoffIcon />}
            label="Handoffs Created"
            value={analytics.handoffsCreated}
            color="info.main"
          />
        </Grid>
      </Grid>

      {/* Sessions Table */}
      {analytics.sessions.length > 0 && (
        <Card variant="outlined">
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Session History
            </Typography>
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Started</TableCell>
                    <TableCell align="right">Duration</TableCell>
                    <TableCell align="right">Tokens</TableCell>
                    <TableCell align="right">Decisions</TableCell>
                    <TableCell align="right">Progress</TableCell>
                    <TableCell align="right">Files</TableCell>
                    <TableCell align="center">Handoff</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {analytics.sessions.map((session: SessionMetrics) => (
                    <TableRow key={session.id} hover>
                      <TableCell>
                        <Typography variant="body2">
                          {formatDate(session.sessionStartedAt)}
                        </Typography>
                        {session.notes && (
                          <Tooltip title={session.notes}>
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              sx={{
                                display: 'block',
                                maxWidth: 200,
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                              }}
                            >
                              {session.notes}
                            </Typography>
                          </Tooltip>
                        )}
                      </TableCell>
                      <TableCell align="right">
                        {session.durationMinutes != null
                          ? formatDuration(session.durationMinutes)
                          : '-'}
                      </TableCell>
                      <TableCell align="right">
                        {session.totalTokens != null ? formatTokens(session.totalTokens) : '-'}
                      </TableCell>
                      <TableCell align="right">{session.decisionsLogged}</TableCell>
                      <TableCell align="right">{session.progressEntriesLogged}</TableCell>
                      <TableCell align="right">{session.filesModified}</TableCell>
                      <TableCell align="center">
                        {session.handoffCreated ? (
                          <Chip label="Yes" size="small" color="success" variant="outlined" />
                        ) : (
                          <Chip label="No" size="small" variant="outlined" />
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      {/* Token Breakdown */}
      {analytics.totalTokens > 0 && (
        <Card variant="outlined" sx={{ mt: 2 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Token Distribution
            </Typography>
            <Box sx={{ mb: 1 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                <Typography variant="body2" color="text.secondary">
                  Input Tokens
                </Typography>
                <Typography variant="body2">
                  {formatTokens(analytics.totalInputTokens)} (
                  {Math.round((analytics.totalInputTokens / analytics.totalTokens) * 100)}%)
                </Typography>
              </Box>
              <LinearProgress
                variant="determinate"
                value={(analytics.totalInputTokens / analytics.totalTokens) * 100}
                sx={{ height: 8, borderRadius: 1 }}
              />
            </Box>
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                <Typography variant="body2" color="text.secondary">
                  Output Tokens
                </Typography>
                <Typography variant="body2">
                  {formatTokens(analytics.totalOutputTokens)} (
                  {Math.round((analytics.totalOutputTokens / analytics.totalTokens) * 100)}%)
                </Typography>
              </Box>
              <LinearProgress
                variant="determinate"
                value={(analytics.totalOutputTokens / analytics.totalTokens) * 100}
                color="secondary"
                sx={{ height: 8, borderRadius: 1 }}
              />
            </Box>
          </CardContent>
        </Card>
      )}
    </Box>
  );
}
