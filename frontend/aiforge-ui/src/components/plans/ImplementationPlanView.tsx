import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Skeleton,
  Alert,
  Card,
  CardContent,
  Chip,
  Button,
  IconButton,
  Collapse,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  Description as PlanIcon,
  CheckCircle as ApprovedIcon,
  Edit as DraftIcon,
  History as SupersededIcon,
  Cancel as RejectedIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  ThumbUp as ApproveIcon,
  ThumbDown as RejectIcon,
  InsertDriveFile as FileIcon,
  Link as DependencyIcon,
  Schedule as EffortIcon,
} from '@mui/icons-material';
import ReactMarkdown from 'react-markdown';
import { plansApi } from '../../api/plans';
import type { ImplementationPlan, PlanStatus } from '../../types';

interface ImplementationPlanViewProps {
  ticketId: string;
}

const statusConfig: Record<PlanStatus, { icon: React.ReactElement; color: 'default' | 'success' | 'warning' | 'error' | 'info'; label: string }> = {
  Draft: { icon: <DraftIcon fontSize="small" />, color: 'warning', label: 'Draft' },
  Approved: { icon: <ApprovedIcon fontSize="small" />, color: 'success', label: 'Approved' },
  Superseded: { icon: <SupersededIcon fontSize="small" />, color: 'default', label: 'Superseded' },
  Rejected: { icon: <RejectedIcon fontSize="small" />, color: 'error', label: 'Rejected' },
};

export default function ImplementationPlanView({ ticketId }: ImplementationPlanViewProps) {
  const [plans, setPlans] = useState<ImplementationPlan[]>([]);
  const [currentPlan, setCurrentPlan] = useState<ImplementationPlan | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showHistory, setShowHistory] = useState(false);

  // Dialog states
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadPlans();
  }, [ticketId]);

  const loadPlans = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [allPlans, current] = await Promise.all([
        plansApi.getByTicket(ticketId),
        plansApi.getCurrent(ticketId),
      ]);
      setPlans(allPlans);
      setCurrentPlan(current);
    } catch (err) {
      setError('Failed to load implementation plans');
      console.error('Error loading plans:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleApprove = async () => {
    if (!currentPlan) return;
    setActionLoading(true);
    try {
      await plansApi.approve(currentPlan.id);
      await loadPlans();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to approve plan');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!currentPlan) return;
    setActionLoading(true);
    try {
      await plansApi.reject(currentPlan.id, { rejectionReason: rejectReason });
      setRejectDialogOpen(false);
      setRejectReason('');
      await loadPlans();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to reject plan');
    } finally {
      setActionLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box>
        <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 1 }} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>;
  }

  if (!currentPlan) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <PlanIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography color="text.secondary">
          No implementation plan yet
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Implementation plans created by Claude will appear here
        </Typography>
      </Box>
    );
  }

  const config = statusConfig[currentPlan.status];
  const historicalPlans = plans.filter(p => p.id !== currentPlan.id);

  return (
    <Box>
      {/* Current Plan Card */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          {/* Header */}
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2, mb: 2 }}>
            <PlanIcon color="primary" sx={{ fontSize: 28 }} />
            <Box sx={{ flex: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                <Typography variant="h6">
                  Implementation Plan v{currentPlan.version}
                </Typography>
                <Chip
                  icon={config.icon}
                  label={config.label}
                  size="small"
                  color={config.color}
                />
              </Box>
              <Typography variant="caption" color="text.secondary">
                Created {new Date(currentPlan.createdAt).toLocaleString()}
                {currentPlan.createdBy && ` by ${currentPlan.createdBy}`}
              </Typography>
            </Box>
          </Box>

          {/* Metadata */}
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 2 }}>
            {currentPlan.estimatedEffort && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <EffortIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Effort: {currentPlan.estimatedEffort}
                </Typography>
              </Box>
            )}
            {currentPlan.affectedFiles.length > 0 && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <FileIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  {currentPlan.affectedFiles.length} file(s) affected
                </Typography>
              </Box>
            )}
            {currentPlan.dependencyTicketIds.length > 0 && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <DependencyIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  {currentPlan.dependencyTicketIds.length} dependencies
                </Typography>
              </Box>
            )}
          </Box>

          {/* Approval Info */}
          {currentPlan.status === 'Approved' && currentPlan.approvedAt && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Approved on {new Date(currentPlan.approvedAt).toLocaleString()}
              {currentPlan.approvedBy && ` by ${currentPlan.approvedBy}`}
            </Alert>
          )}

          {/* Rejection Info */}
          {currentPlan.status === 'Rejected' && (
            <Alert severity="error" sx={{ mb: 2 }}>
              Rejected on {new Date(currentPlan.rejectedAt!).toLocaleString()}
              {currentPlan.rejectedBy && ` by ${currentPlan.rejectedBy}`}
              {currentPlan.rejectionReason && (
                <Typography variant="body2" sx={{ mt: 1 }}>
                  Reason: {currentPlan.rejectionReason}
                </Typography>
              )}
            </Alert>
          )}

          <Divider sx={{ my: 2 }} />

          {/* Plan Content */}
          <Box sx={{
            '& pre': {
              bgcolor: 'grey.100',
              p: 2,
              borderRadius: 1,
              overflow: 'auto',
              fontSize: '0.875rem',
            },
            '& code': {
              bgcolor: 'grey.100',
              px: 0.5,
              borderRadius: 0.5,
              fontSize: '0.875rem',
            },
            '& h1, & h2, & h3, & h4': {
              mt: 2,
              mb: 1,
            },
            '& ul, & ol': {
              pl: 3,
            },
          }}>
            <ReactMarkdown>{currentPlan.content}</ReactMarkdown>
          </Box>

          {/* Affected Files List */}
          {currentPlan.affectedFiles.length > 0 && (
            <Box sx={{ mt: 3 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Affected Files
              </Typography>
              <List dense disablePadding>
                {currentPlan.affectedFiles.map((file, index) => (
                  <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <FileIcon fontSize="small" color="action" />
                    </ListItemIcon>
                    <ListItemText
                      primary={file}
                      primaryTypographyProps={{ variant: 'body2', fontFamily: 'monospace' }}
                    />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}

          {/* Action Buttons */}
          {currentPlan.status === 'Draft' && (
            <Box sx={{ display: 'flex', gap: 1, mt: 3 }}>
              <Button
                variant="contained"
                color="success"
                startIcon={<ApproveIcon />}
                onClick={handleApprove}
                disabled={actionLoading}
              >
                Approve Plan
              </Button>
              <Button
                variant="outlined"
                color="error"
                startIcon={<RejectIcon />}
                onClick={() => setRejectDialogOpen(true)}
                disabled={actionLoading}
              >
                Reject
              </Button>
            </Box>
          )}

          {currentPlan.status === 'Approved' && (
            <Box sx={{ mt: 3 }}>
              <Typography variant="body2" color="text.secondary">
                To create a new version, use Claude to supersede this plan.
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* Version History */}
      {historicalPlans.length > 0 && (
        <Box>
          <Button
            size="small"
            startIcon={showHistory ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            onClick={() => setShowHistory(!showHistory)}
            sx={{ mb: 1 }}
          >
            {showHistory ? 'Hide' : 'Show'} Version History ({historicalPlans.length})
          </Button>
          <Collapse in={showHistory}>
            {historicalPlans.map((plan) => (
              <PlanHistoryCard key={plan.id} plan={plan} />
            ))}
          </Collapse>
        </Box>
      )}

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onClose={() => setRejectDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Reject Implementation Plan</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Reason for rejection (optional)"
            fullWidth
            multiline
            rows={3}
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleReject} color="error" disabled={actionLoading}>
            Reject Plan
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

// Sub-component for historical plan cards
function PlanHistoryCard({ plan }: { plan: ImplementationPlan }) {
  const [expanded, setExpanded] = useState(false);
  const config = statusConfig[plan.status];

  return (
    <Card sx={{ mb: 1, opacity: 0.8 }}>
      <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="body2" fontWeight={500}>
            v{plan.version}
          </Typography>
          <Chip
            icon={config.icon}
            label={config.label}
            size="small"
            color={config.color}
            sx={{ height: 24 }}
          />
          <Typography variant="caption" color="text.secondary" sx={{ flex: 1 }}>
            {new Date(plan.createdAt).toLocaleString()}
          </Typography>
          <IconButton size="small" onClick={() => setExpanded(!expanded)}>
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
        <Collapse in={expanded}>
          <Box sx={{ mt: 2, maxHeight: 300, overflow: 'auto' }}>
            <ReactMarkdown>{plan.content}</ReactMarkdown>
          </Box>
        </Collapse>
      </CardContent>
    </Card>
  );
}
