import { useEffect, useState } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Skeleton,
  Alert,
  Link,
  LinearProgress,
  Tooltip,
} from '@mui/material';
import {
  Analytics as AnalyticsIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
  Code as CodeIcon,
  Timer as TimerIcon,
  Token as TokenIcon,
  Psychology as DecisionIcon,
  Description as HandoffIcon,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { analyticsApi } from '../api/analytics';
import type { AnalyticsDashboard as DashboardData, LowConfidenceDecision, AnalyticsHotFile } from '../types';

function formatNumber(num: number): string {
  if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
  if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
  return num.toString();
}

function formatDuration(minutes: number): string {
  if (minutes < 60) return `${minutes}m`;
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
}

function ConfidenceBar({ value }: { value: number }) {
  const color = value >= 70 ? 'success' : value >= 50 ? 'warning' : 'error';
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Box sx={{ flex: 1, minWidth: 60 }}>
        <LinearProgress
          variant="determinate"
          value={value}
          color={color}
          sx={{ height: 8, borderRadius: 4 }}
        />
      </Box>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 35 }}>
        {value}%
      </Typography>
    </Box>
  );
}

export default function AnalyticsDashboard() {
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await analyticsApi.getDashboard({
        recentActivityLimit: 10,
        topHotFilesLimit: 5,
        lowConfidenceLimit: 5,
      });
      setDashboard(data);
    } catch (err) {
      setError('Failed to load analytics dashboard');
      console.error('Error loading analytics:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (error) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AnalyticsIcon />
          Analytics
        </Typography>
        <Alert severity="error" sx={{ mt: 2 }}>
          {error}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <AnalyticsIcon />
        Analytics & Insights
      </Typography>

      {/* Overview Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {/* Tickets Overview */}
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" variant="body2" gutterBottom>
                Total Tickets
              </Typography>
              {isLoading ? (
                <Skeleton variant="text" width={80} height={48} />
              ) : (
                <>
                  <Typography variant="h4">{dashboard?.totalTickets ?? 0}</Typography>
                  <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                    <Chip
                      label={`${dashboard?.ticketsInProgress ?? 0} in progress`}
                      size="small"
                      color="primary"
                      variant="outlined"
                    />
                    <Chip
                      label={`${dashboard?.ticketsCompleted ?? 0} done`}
                      size="small"
                      color="success"
                      variant="outlined"
                    />
                  </Box>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Sessions & Time */}
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <TimerIcon color="primary" />
                <Typography color="text.secondary" variant="body2">
                  Work Sessions
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={80} height={48} />
              ) : (
                <>
                  <Typography variant="h4">{dashboard?.totalSessions ?? 0}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatDuration(dashboard?.totalMinutesWorked ?? 0)} total time
                  </Typography>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Tokens Used */}
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <TokenIcon color="secondary" />
                <Typography color="text.secondary" variant="body2">
                  Tokens Used
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={80} height={48} />
              ) : (
                <>
                  <Typography variant="h4">{formatNumber(dashboard?.totalTokensUsed ?? 0)}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {dashboard?.handoffsCreated ?? 0} handoffs created
                  </Typography>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Confidence Score */}
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <DecisionIcon color="info" />
                <Typography color="text.secondary" variant="body2">
                  Avg Confidence
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={80} height={48} />
              ) : (
                <>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="h4">
                      {Math.round(dashboard?.overallAverageConfidence ?? 0)}%
                    </Typography>
                    {(dashboard?.overallAverageConfidence ?? 0) >= 70 ? (
                      <TrendingUpIcon color="success" />
                    ) : (dashboard?.overallAverageConfidence ?? 0) >= 50 ? (
                      <TrendingUpIcon color="warning" />
                    ) : (
                      <TrendingDownIcon color="error" />
                    )}
                  </Box>
                  <Typography variant="body2" color="text.secondary">
                    {dashboard?.totalDecisions ?? 0} decisions logged
                  </Typography>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Main Content Grid */}
      <Grid container spacing={3}>
        {/* Low Confidence Decisions */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <WarningIcon color="warning" />
                <Typography variant="h6">Low Confidence Decisions</Typography>
                {!isLoading && (
                  <Chip
                    label={dashboard?.lowConfidenceDecisionCount ?? 0}
                    size="small"
                    color="warning"
                  />
                )}
              </Box>

              {isLoading ? (
                Array.from({ length: 3 }).map((_, i) => (
                  <Skeleton key={i} variant="rectangular" height={60} sx={{ mb: 1 }} />
                ))
              ) : (dashboard?.recentLowConfidenceDecisions?.length ?? 0) === 0 ? (
                <Box sx={{ textAlign: 'center', py: 3 }}>
                  <CheckIcon color="success" sx={{ fontSize: 48, mb: 1 }} />
                  <Typography color="text.secondary">
                    No low confidence decisions needing review
                  </Typography>
                </Box>
              ) : (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Ticket</TableCell>
                        <TableCell>Decision</TableCell>
                        <TableCell width={100}>Confidence</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {dashboard?.recentLowConfidenceDecisions?.map((decision: LowConfidenceDecision) => (
                        <TableRow key={decision.reasoningLogId} hover>
                          <TableCell>
                            <Link
                              component={RouterLink}
                              to={`/tickets/${decision.ticketKey}`}
                              sx={{ fontWeight: 500 }}
                            >
                              {decision.ticketKey}
                            </Link>
                          </TableCell>
                          <TableCell>
                            <Tooltip title={decision.decisionPoint}>
                              <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                                {decision.decisionPoint}
                              </Typography>
                            </Tooltip>
                          </TableCell>
                          <TableCell>
                            <ConfidenceBar value={decision.confidencePercent} />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Hot Files */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <CodeIcon color="error" />
                <Typography variant="h6">Hot Files</Typography>
              </Box>

              {isLoading ? (
                Array.from({ length: 3 }).map((_, i) => (
                  <Skeleton key={i} variant="rectangular" height={50} sx={{ mb: 1 }} />
                ))
              ) : (dashboard?.topHotFiles?.length ?? 0) === 0 ? (
                <Box sx={{ textAlign: 'center', py: 3 }}>
                  <CodeIcon color="disabled" sx={{ fontSize: 48, mb: 1 }} />
                  <Typography color="text.secondary">
                    No file changes tracked yet
                  </Typography>
                </Box>
              ) : (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>File</TableCell>
                        <TableCell align="right">Changes</TableCell>
                        <TableCell align="right">Tickets</TableCell>
                        <TableCell align="right">Churn</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {dashboard?.topHotFiles?.map((file: AnalyticsHotFile) => (
                        <TableRow key={file.filePath} hover>
                          <TableCell>
                            <Tooltip title={file.filePath}>
                              <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                                {file.filePath.split('/').pop()}
                              </Typography>
                            </Tooltip>
                          </TableCell>
                          <TableCell align="right">
                            <Chip label={file.modificationCount} size="small" color="primary" />
                          </TableCell>
                          <TableCell align="right">{file.ticketCount}</TableCell>
                          <TableCell align="right">
                            <Typography variant="body2" color="success.main">
                              +{file.totalLinesAdded}
                            </Typography>
                            <Typography variant="body2" color="error.main">
                              -{file.totalLinesRemoved}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Technical Debt Summary */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <WarningIcon color="warning" />
                <Typography variant="h6">Technical Debt</Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="rectangular" height={100} />
              ) : (
                <Box sx={{ textAlign: 'center', py: 2 }}>
                  <Typography variant="h3" color="warning.main">
                    {dashboard?.openTechnicalDebtCount ?? 0}
                  </Typography>
                  <Typography color="text.secondary">Open Items</Typography>
                  <Link component={RouterLink} to="/debt" sx={{ mt: 1, display: 'block' }}>
                    View Backlog
                  </Link>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Activity */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <HandoffIcon color="info" />
                <Typography variant="h6">Recent Activity</Typography>
              </Box>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <Skeleton key={i} variant="rectangular" height={40} sx={{ mb: 1 }} />
                ))
              ) : (dashboard?.recentActivity?.length ?? 0) === 0 ? (
                <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                  No recent activity
                </Typography>
              ) : (
                <TableContainer>
                  <Table size="small">
                    <TableBody>
                      {dashboard?.recentActivity?.map((activity, idx) => (
                        <TableRow key={idx} hover>
                          <TableCell width={100}>
                            <Chip
                              label={activity.activityType}
                              size="small"
                              color={
                                activity.activityType === 'Session'
                                  ? 'primary'
                                  : activity.activityType === 'Decision'
                                  ? 'info'
                                  : 'default'
                              }
                              variant="outlined"
                            />
                          </TableCell>
                          <TableCell>
                            {activity.ticketKey && (
                              <Link
                                component={RouterLink}
                                to={`/tickets/${activity.ticketKey}`}
                                sx={{ mr: 1 }}
                              >
                                {activity.ticketKey}
                              </Link>
                            )}
                            <Typography variant="body2" component="span" color="text.secondary">
                              {activity.description}
                            </Typography>
                          </TableCell>
                          <TableCell width={150} align="right">
                            <Typography variant="caption" color="text.secondary">
                              {new Date(activity.timestamp).toLocaleString()}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Generated timestamp */}
      {dashboard && (
        <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
          Generated at {new Date(dashboard.generatedAt).toLocaleString()}
        </Typography>
      )}
    </Box>
  );
}
