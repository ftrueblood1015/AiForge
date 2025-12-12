import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  CardActionArea,
  Grid,
  Skeleton,
  Alert,
  Chip,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Pagination,
} from '@mui/material';
import {
  Description as HandoffIcon,
  Search as SearchIcon,
  Warning as WarningIcon,
  Celebration as MilestoneIcon,
  ExitToApp as SessionEndIcon,
  Storage as ContextDumpIcon,
} from '@mui/icons-material';
import { handoffsApi } from '../api/handoffs';
import type { HandoffDocument, HandoffType } from '../types';

const typeConfig: Record<HandoffType, { icon: React.ReactNode; color: 'default' | 'primary' | 'warning' | 'info'; label: string }> = {
  SessionEnd: { icon: <SessionEndIcon />, color: 'default', label: 'Session End' },
  Blocker: { icon: <WarningIcon />, color: 'warning', label: 'Blocker' },
  Milestone: { icon: <MilestoneIcon />, color: 'primary', label: 'Milestone' },
  ContextDump: { icon: <ContextDumpIcon />, color: 'info', label: 'Context Dump' },
};

const TYPES: (HandoffType | 'all')[] = ['all', 'SessionEnd', 'Blocker', 'Milestone', 'ContextDump'];
const ITEMS_PER_PAGE = 12;

export default function HandoffList() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [handoffs, setHandoffs] = useState<HandoffDocument[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [searchTerm, setSearchTerm] = useState(searchParams.get('search') || '');
  const [typeFilter, setTypeFilter] = useState<HandoffType | 'all'>(
    (searchParams.get('type') as HandoffType) || 'all'
  );
  const [page, setPage] = useState(1);

  useEffect(() => {
    loadHandoffs();
  }, []);

  const loadHandoffs = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await handoffsApi.search({});
      setHandoffs(data);
    } catch (err) {
      setError('Failed to load handoffs');
      console.error('Error loading handoffs:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Filter handoffs
  const filteredHandoffs = handoffs.filter((h) => {
    const matchesSearch =
      !searchTerm ||
      h.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      h.summary.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = typeFilter === 'all' || h.type === typeFilter;
    return matchesSearch && matchesType;
  });

  // Paginate
  const totalPages = Math.ceil(filteredHandoffs.length / ITEMS_PER_PAGE);
  const paginatedHandoffs = filteredHandoffs.slice(
    (page - 1) * ITEMS_PER_PAGE,
    page * ITEMS_PER_PAGE
  );

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setPage(1);
    const params = new URLSearchParams(searchParams);
    if (value) {
      params.set('search', value);
    } else {
      params.delete('search');
    }
    setSearchParams(params);
  };

  const handleTypeChange = (value: HandoffType | 'all') => {
    setTypeFilter(value);
    setPage(1);
    const params = new URLSearchParams(searchParams);
    if (value !== 'all') {
      params.set('type', value);
    } else {
      params.delete('type');
    }
    setSearchParams(params);
  };

  if (error) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom>
          Handoffs
        </Typography>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Handoffs
      </Typography>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                size="small"
                placeholder="Search handoffs..."
                value={searchTerm}
                onChange={(e) => handleSearchChange(e.target.value)}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon color="action" />
                      </InputAdornment>
                    ),
                  },
                }}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Type</InputLabel>
                <Select
                  value={typeFilter}
                  label="Type"
                  onChange={(e) => handleTypeChange(e.target.value as HandoffType | 'all')}
                >
                  {TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type === 'all' ? 'All Types' : typeConfig[type].label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 3 }}>
              <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'right' }}>
                {filteredHandoffs.length} handoff{filteredHandoffs.length !== 1 ? 's' : ''}
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Loading State */}
      {isLoading ? (
        <Grid container spacing={2}>
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Grid key={i} size={{ xs: 12, md: 6, lg: 4 }}>
              <Skeleton variant="rectangular" height={180} sx={{ borderRadius: 2 }} />
            </Grid>
          ))}
        </Grid>
      ) : filteredHandoffs.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <HandoffIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No Handoffs Found
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {searchTerm || typeFilter !== 'all'
                ? 'Try adjusting your filters'
                : 'Handoff documents will appear here when created'}
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <>
          {/* Handoff Cards */}
          <Grid container spacing={2}>
            {paginatedHandoffs.map((handoff) => {
              const config = typeConfig[handoff.type];
              return (
                <Grid key={handoff.id} size={{ xs: 12, md: 6, lg: 4 }}>
                  <Card
                    sx={{
                      height: '100%',
                      opacity: handoff.isActive ? 1 : 0.7,
                      border: handoff.type === 'Blocker' ? 2 : 0,
                      borderColor: 'warning.main',
                    }}
                  >
                    <CardActionArea
                      onClick={() => navigate(`/handoffs/${handoff.id}`)}
                      sx={{ height: '100%', display: 'flex', alignItems: 'flex-start', p: 0 }}
                    >
                      <CardContent sx={{ width: '100%' }}>
                        {/* Header */}
                        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1, mb: 1 }}>
                          <Box sx={{ color: `${config.color}.main`, mt: 0.25 }}>{config.icon}</Box>
                          <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography
                              variant="subtitle1"
                              fontWeight={600}
                              sx={{
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                              }}
                            >
                              {handoff.title}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {new Date(handoff.createdAt).toLocaleString()}
                            </Typography>
                          </Box>
                        </Box>

                        {/* Chips */}
                        <Box sx={{ display: 'flex', gap: 0.5, mb: 1.5, flexWrap: 'wrap' }}>
                          <Chip label={config.label} size="small" color={config.color} />
                          {!handoff.isActive && (
                            <Chip label="Superseded" size="small" variant="outlined" />
                          )}
                        </Box>

                        {/* Summary */}
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{
                            display: '-webkit-box',
                            WebkitLineClamp: 3,
                            WebkitBoxOrient: 'vertical',
                            overflow: 'hidden',
                          }}
                        >
                          {handoff.summary}
                        </Typography>

                        {/* Context Preview */}
                        {handoff.structuredContext && (
                          <Box sx={{ display: 'flex', gap: 0.5, mt: 1.5, flexWrap: 'wrap' }}>
                            {handoff.structuredContext.blockers &&
                              handoff.structuredContext.blockers.length > 0 && (
                                <Chip
                                  label={`${handoff.structuredContext.blockers.length} blocker(s)`}
                                  size="small"
                                  color="error"
                                  variant="outlined"
                                />
                              )}
                            {handoff.structuredContext.warnings &&
                              handoff.structuredContext.warnings.length > 0 && (
                                <Chip
                                  label={`${handoff.structuredContext.warnings.length} warning(s)`}
                                  size="small"
                                  color="warning"
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
                      </CardContent>
                    </CardActionArea>
                  </Card>
                </Grid>
              );
            })}
          </Grid>

          {/* Pagination */}
          {totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination
                count={totalPages}
                page={page}
                onChange={(_, value) => setPage(value)}
                color="primary"
              />
            </Box>
          )}
        </>
      )}
    </Box>
  );
}
