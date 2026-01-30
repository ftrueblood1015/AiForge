import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Alert,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  Divider,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Skeleton,
  MenuItem,
} from '@mui/material';
import {
  ArrowBack as BackIcon,
  Pause as PauseIcon,
  PlayArrow as ResumeIcon,
  Cancel as CancelIcon,
  Refresh as RefreshIcon,
  Warning as InterventionIcon,
} from '@mui/icons-material';
import { skillChainExecutionsApi } from '../api/skillChains';
import { ExecutionTimeline } from '../components/skillChains';
import type { SkillChainExecution, ChainExecutionStatus } from '../types';

const statusColors: Record<ChainExecutionStatus, 'default' | 'primary' | 'success' | 'error' | 'warning' | 'info'> = {
  Pending: 'default',
  Running: 'primary',
  Paused: 'warning',
  Completed: 'success',
  Failed: 'error',
  Cancelled: 'default',
};

export default function ExecutionDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [execution, setExecution] = useState<SkillChainExecution | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  // Dialog states
  const [pauseDialogOpen, setPauseDialogOpen] = useState(false);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [interventionDialogOpen, setInterventionDialogOpen] = useState(false);
  const [pauseReason, setPauseReason] = useState('');
  const [cancelReason, setCancelReason] = useState('');
  const [resolution, setResolution] = useState('');
  const [nextAction, setNextAction] = useState<'Retry' | 'GoToLink' | 'Complete' | 'Escalate'>('Retry');

  const fetchExecution = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await skillChainExecutionsApi.getById(id);
      setExecution(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load execution');
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchExecution();
  }, [fetchExecution]);

  // Auto-refresh for running executions
  useEffect(() => {
    if (execution?.status === 'Running') {
      const interval = setInterval(fetchExecution, 5000);
      return () => clearInterval(interval);
    }
  }, [execution?.status, fetchExecution]);

  const handleBack = () => {
    navigate('/skill-chains');
  };

  const handlePause = async () => {
    if (!execution) return;
    setActionLoading(true);
    try {
      const updated = await skillChainExecutionsApi.pause(execution.id, { reason: pauseReason });
      setExecution(updated);
      setPauseDialogOpen(false);
      setPauseReason('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to pause execution');
    } finally {
      setActionLoading(false);
    }
  };

  const handleResume = async () => {
    if (!execution) return;
    setActionLoading(true);
    try {
      const updated = await skillChainExecutionsApi.resume(execution.id);
      setExecution(updated);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to resume execution');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancel = async () => {
    if (!execution) return;
    setActionLoading(true);
    try {
      const updated = await skillChainExecutionsApi.cancel(execution.id, { reason: cancelReason });
      setExecution(updated);
      setCancelDialogOpen(false);
      setCancelReason('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to cancel execution');
    } finally {
      setActionLoading(false);
    }
  };

  const handleResolveIntervention = async () => {
    if (!execution) return;
    setActionLoading(true);
    try {
      const updated = await skillChainExecutionsApi.resolveIntervention(execution.id, {
        resolution,
        nextAction,
        resolvedBy: 'User',
      });
      setExecution(updated);
      setInterventionDialogOpen(false);
      setResolution('');
      setNextAction('Retry');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to resolve intervention');
    } finally {
      setActionLoading(false);
    }
  };

  if (isLoading && !execution) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <Skeleton variant="circular" width={40} height={40} />
          <Skeleton variant="text" width={300} height={40} />
        </Box>
        <Skeleton variant="rounded" height={150} sx={{ mb: 2 }} />
        <Skeleton variant="rounded" height={400} />
      </Box>
    );
  }

  if (!execution && !isLoading) {
    return (
      <Box>
        <Button startIcon={<BackIcon />} onClick={handleBack} sx={{ mb: 2 }}>
          Back to Skill Chains
        </Button>
        <Alert severity="error">
          Execution not found
        </Alert>
      </Box>
    );
  }

  if (!execution) return null;

  const canPause = execution.status === 'Running';
  const canResume = execution.status === 'Paused';
  const canCancel = execution.status === 'Running' || execution.status === 'Paused';
  const isFinished = ['Completed', 'Failed', 'Cancelled'].includes(execution.status);

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={handleBack}>
            <BackIcon />
          </IconButton>
          <Box>
            <Typography variant="h5" fontWeight={600}>
              {execution.chainName}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Execution {execution.id.substring(0, 8)}...
            </Typography>
          </Box>
          <Chip
            label={execution.status}
            color={statusColors[execution.status]}
            size="small"
          />
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={fetchExecution} disabled={isLoading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          {canPause && (
            <Tooltip title="Pause execution">
              <IconButton onClick={() => setPauseDialogOpen(true)} disabled={actionLoading}>
                <PauseIcon />
              </IconButton>
            </Tooltip>
          )}
          {canResume && (
            <Tooltip title="Resume execution">
              <IconButton onClick={handleResume} disabled={actionLoading} color="primary">
                <ResumeIcon />
              </IconButton>
            </Tooltip>
          )}
          {canCancel && (
            <Tooltip title="Cancel execution">
              <IconButton onClick={() => setCancelDialogOpen(true)} disabled={actionLoading} color="error">
                <CancelIcon />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Intervention Alert */}
      {execution.requiresHumanIntervention && (
        <Alert
          severity="warning"
          icon={<InterventionIcon />}
          sx={{ mb: 2 }}
          action={
            <Button color="inherit" size="small" onClick={() => setInterventionDialogOpen(true)}>
              Resolve
            </Button>
          }
        >
          <Typography variant="subtitle2">Human Intervention Required</Typography>
          <Typography variant="body2">
            {execution.interventionReason || 'This execution requires human intervention to proceed.'}
          </Typography>
        </Alert>
      )}

      {/* Execution Info */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Execution Details
        </Typography>

        <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', mb: 2 }}>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Started
            </Typography>
            <Typography variant="body2">
              {new Date(execution.startedAt).toLocaleString()}
            </Typography>
          </Box>
          {execution.completedAt && (
            <Box>
              <Typography variant="caption" color="text.secondary">
                Completed
              </Typography>
              <Typography variant="body2">
                {new Date(execution.completedAt).toLocaleString()}
              </Typography>
            </Box>
          )}
          <Box>
            <Typography variant="caption" color="text.secondary">
              Current Link
            </Typography>
            <Typography variant="body2">
              {execution.currentLinkName || 'N/A'} (Position {(execution.currentLinkPosition ?? 0) + 1})
            </Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Total Failures
            </Typography>
            <Typography variant="body2" color={execution.totalFailureCount > 0 ? 'error.main' : 'inherit'}>
              {execution.totalFailureCount}
            </Typography>
          </Box>
          {execution.startedBy && (
            <Box>
              <Typography variant="caption" color="text.secondary">
                Started By
              </Typography>
              <Typography variant="body2">{execution.startedBy}</Typography>
            </Box>
          )}
        </Box>

        {execution.ticketId && (
          <Box sx={{ mt: 1 }}>
            <Typography variant="caption" color="text.secondary">
              Linked Ticket
            </Typography>
            <Typography variant="body2">{execution.ticketId}</Typography>
          </Box>
        )}
      </Paper>

      {/* Execution Timeline */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Execution Timeline
        </Typography>
        <Divider sx={{ mb: 2 }} />
        <ExecutionTimeline linkExecutions={execution.linkExecutions || []} />
      </Paper>

      {/* Pause Dialog */}
      <Dialog open={pauseDialogOpen} onClose={() => setPauseDialogOpen(false)}>
        <DialogTitle>Pause Execution</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Pausing will stop the execution after the current link completes.
          </Typography>
          <TextField
            label="Reason (optional)"
            fullWidth
            multiline
            rows={2}
            value={pauseReason}
            onChange={(e) => setPauseReason(e.target.value)}
            placeholder="Why are you pausing this execution?"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPauseDialogOpen(false)}>Cancel</Button>
          <Button onClick={handlePause} variant="contained" disabled={actionLoading}>
            Pause
          </Button>
        </DialogActions>
      </Dialog>

      {/* Cancel Dialog */}
      <Dialog open={cancelDialogOpen} onClose={() => setCancelDialogOpen(false)}>
        <DialogTitle>Cancel Execution</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            This will permanently stop the execution. This action cannot be undone.
          </Typography>
          <TextField
            label="Reason"
            fullWidth
            required
            multiline
            rows={2}
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder="Why are you cancelling this execution?"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelDialogOpen(false)}>Back</Button>
          <Button
            onClick={handleCancel}
            variant="contained"
            color="error"
            disabled={actionLoading || !cancelReason.trim()}
          >
            Cancel Execution
          </Button>
        </DialogActions>
      </Dialog>

      {/* Intervention Resolution Dialog */}
      <Dialog open={interventionDialogOpen} onClose={() => setInterventionDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Resolve Intervention</DialogTitle>
        <DialogContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            {execution.interventionReason || 'This execution requires human intervention.'}
          </Alert>
          <TextField
            label="Resolution"
            fullWidth
            required
            multiline
            rows={3}
            value={resolution}
            onChange={(e) => setResolution(e.target.value)}
            placeholder="Describe what you did to resolve this..."
            sx={{ mb: 2 }}
          />
          <TextField
            select
            label="Next Action"
            fullWidth
            value={nextAction}
            onChange={(e) => setNextAction(e.target.value as typeof nextAction)}
          >
            <MenuItem value="Retry">Retry current link</MenuItem>
            <MenuItem value="Complete">Mark as complete</MenuItem>
            <MenuItem value="Escalate">Escalate further</MenuItem>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setInterventionDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleResolveIntervention}
            variant="contained"
            disabled={actionLoading || !resolution.trim()}
          >
            Resolve
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
