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
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Skeleton,
  Alert,
  Link,
} from '@mui/material';
import {
  Warning as DebtIcon,
  CheckCircle as ResolvedIcon,
  Error as CriticalIcon,
  ErrorOutline as HighIcon,
  Info as MediumIcon,
  CheckCircleOutline as LowIcon,
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { technicalDebtApi } from '../api/codeIntelligence';
import type { TechnicalDebt, DebtSummaryResponse, DebtSeverity, DebtStatus, DebtCategory } from '../types';

const severityIcons: Record<DebtSeverity, React.ReactElement> = {
  Critical: <CriticalIcon color="error" />,
  High: <HighIcon color="warning" />,
  Medium: <MediumIcon color="info" />,
  Low: <LowIcon color="success" />,
};

const severityColors: Record<DebtSeverity, 'error' | 'warning' | 'info' | 'success'> = {
  Critical: 'error',
  High: 'warning',
  Medium: 'info',
  Low: 'success',
};

const statusColors: Record<DebtStatus, 'default' | 'primary' | 'success' | 'warning'> = {
  Open: 'default',
  InProgress: 'primary',
  Resolved: 'success',
  Accepted: 'warning',
};

export default function TechnicalDebtDashboard() {
  const [debts, setDebts] = useState<TechnicalDebt[]>([]);
  const [summary, setSummary] = useState<DebtSummaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filters
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  const [severityFilter, setSeverityFilter] = useState<string>('');

  useEffect(() => {
    loadData();
  }, [statusFilter, categoryFilter, severityFilter]);

  const loadData = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [backlog, summaryData] = await Promise.all([
        technicalDebtApi.getBacklog(
          statusFilter || undefined,
          categoryFilter || undefined,
          severityFilter || undefined
        ),
        technicalDebtApi.getSummary(),
      ]);
      setDebts(backlog.items);
      setSummary(summaryData);
    } catch (err) {
      setError('Failed to load technical debt data');
      console.error('Error loading technical debt:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const categories: DebtCategory[] = ['Performance', 'Security', 'Maintainability', 'Testing', 'Documentation', 'Architecture'];
  const severities: DebtSeverity[] = ['Critical', 'High', 'Medium', 'Low'];
  const statuses: DebtStatus[] = ['Open', 'InProgress', 'Resolved', 'Accepted'];

  if (error) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom>
          Technical Debt
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
        <DebtIcon />
        Technical Debt
      </Typography>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <DebtIcon color="warning" />
                <Typography color="text.secondary" variant="body2">
                  Open Debt
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{summary?.totalOpen ?? 0}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <ResolvedIcon color="success" />
                <Typography color="text.secondary" variant="body2">
                  Resolved
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{summary?.totalResolved ?? 0}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" variant="body2" gutterBottom>
                By Category
              </Typography>
              {isLoading ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {summary?.byCategory &&
                    Object.entries(summary.byCategory).map(([cat, count]) => (
                      <Chip
                        key={cat}
                        label={`${cat}: ${count}`}
                        size="small"
                        variant="outlined"
                      />
                    ))}
                  {(!summary?.byCategory || Object.keys(summary.byCategory).length === 0) && (
                    <Typography variant="body2" color="text.secondary">
                      No open debt
                    </Typography>
                  )}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" variant="body2" gutterBottom>
                By Severity
              </Typography>
              {isLoading ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {summary?.bySeverity &&
                    Object.entries(summary.bySeverity).map(([sev, count]) => (
                      <Chip
                        key={sev}
                        label={`${sev}: ${count}`}
                        size="small"
                        color={severityColors[sev as DebtSeverity]}
                      />
                    ))}
                  {(!summary?.bySeverity || Object.keys(summary.bySeverity).length === 0) && (
                    <Typography variant="body2" color="text.secondary">
                      No open debt
                    </Typography>
                  )}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 4 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={statusFilter}
                label="Status"
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <MenuItem value="">All</MenuItem>
                {statuses.map((s) => (
                  <MenuItem key={s} value={s}>
                    {s}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Category</InputLabel>
              <Select
                value={categoryFilter}
                label="Category"
                onChange={(e) => setCategoryFilter(e.target.value)}
              >
                <MenuItem value="">All</MenuItem>
                {categories.map((c) => (
                  <MenuItem key={c} value={c}>
                    {c}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Severity</InputLabel>
              <Select
                value={severityFilter}
                label="Severity"
                onChange={(e) => setSeverityFilter(e.target.value)}
              >
                <MenuItem value="">All</MenuItem>
                {severities.map((s) => (
                  <MenuItem key={s} value={s}>
                    {s}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>

      {/* Debt Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Severity</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Ticket</TableCell>
              <TableCell>Created</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton /></TableCell>
                  <TableCell><Skeleton /></TableCell>
                  <TableCell><Skeleton /></TableCell>
                  <TableCell><Skeleton /></TableCell>
                  <TableCell><Skeleton /></TableCell>
                  <TableCell><Skeleton /></TableCell>
                </TableRow>
              ))
            ) : debts.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography color="text.secondary" sx={{ py: 2 }}>
                    No technical debt found
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              debts.map((debt) => (
                <TableRow key={debt.id} hover>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {severityIcons[debt.severity]}
                      {debt.severity}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography fontWeight={500}>{debt.title}</Typography>
                    {debt.description && (
                      <Typography variant="caption" color="text.secondary" display="block">
                        {debt.description.length > 100
                          ? `${debt.description.substring(0, 100)}...`
                          : debt.description}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Chip label={debt.category} size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={debt.status}
                      size="small"
                      color={statusColors[debt.status]}
                    />
                  </TableCell>
                  <TableCell>
                    {debt.originatingTicketKey && (
                      <Link
                        component={RouterLink}
                        to={`/tickets/${debt.originatingTicketId}`}
                      >
                        {debt.originatingTicketKey}
                      </Link>
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {new Date(debt.createdAt).toLocaleDateString()}
                    </Typography>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
