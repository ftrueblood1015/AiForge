import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Skeleton,
  Alert,
  Breadcrumbs,
  Link,
  LinearProgress,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Divider,
  Snackbar,
  ToggleButtonGroup,
  ToggleButton,
  Tooltip,
} from '@mui/material';
import {
  NavigateNext as NavigateNextIcon,
  MoreVert as MoreIcon,
  Lock as LockIcon,
  LockOpen as UnlockIcon,
  Delete as DeleteIcon,
  Archive as ArchiveIcon,
  ViewCompact as SimpleViewIcon,
  Layers as TieredViewIcon,
} from '@mui/icons-material';
import { useParams, Link as RouterLink, useNavigate } from 'react-router-dom';
import { useQueueStore } from '../stores/queueStore';
import {
  QueueStatusChip,
  DraggableQueueItemList,
  ContextHelperPanel,
  CheckoutDialog,
  TieredContextView,
} from '../components/queues';
import { workQueuesApi } from '../api/workQueues';
import type { WorkQueueItemStatus, UpdateContextRequest, TieredContextResponse } from '../types';

export default function QueueDetail() {
  const navigate = useNavigate();
  const { projectId, queueId } = useParams<{ projectId: string; queueId: string }>();
  const {
    currentQueue,
    isLoading,
    error,
    fetchQueue,
    checkoutQueue,
    releaseQueue,
    updateItemStatus,
    reorderItems,
    updateContext,
    deleteQueue,
    clearError,
  } = useQueueStore();

  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [snackbar, setSnackbar] = useState<{ message: string; severity: 'success' | 'error' } | null>(null);
  const [checkoutDialogOpen, setCheckoutDialogOpen] = useState(false);
  const [checkoutConflict, setCheckoutConflict] = useState<{ checkedOutBy: string; expiresAt?: string } | null>(null);
  const [contextViewMode, setContextViewMode] = useState<'simple' | 'tiered'>('simple');

  useEffect(() => {
    if (projectId && queueId) {
      fetchQueue(projectId, queueId);
    }
  }, [projectId, queueId, fetchQueue]);

  const handleOpenCheckoutDialog = () => {
    // Check if there's an existing checkout by someone else
    if (currentQueue?.checkedOutBy) {
      setCheckoutConflict({
        checkedOutBy: currentQueue.checkedOutBy,
        expiresAt: currentQueue.checkoutExpiresAt || undefined,
      });
    } else {
      setCheckoutConflict(null);
    }
    setCheckoutDialogOpen(true);
  };

  const handleCheckout = async (durationMinutes?: number) => {
    if (!projectId || !queueId) return;
    await checkoutQueue(projectId, queueId, durationMinutes);
    setSnackbar({ message: 'Queue checked out successfully', severity: 'success' });
  };

  const handleRelease = async () => {
    if (!projectId || !queueId) return;
    try {
      await releaseQueue(projectId, queueId);
      setSnackbar({ message: 'Queue released', severity: 'success' });
    } catch (err) {
      setSnackbar({ message: (err as Error).message, severity: 'error' });
    }
  };

  const handleReorder = async (itemIds: string[]) => {
    if (!projectId || !queueId) return;
    try {
      await reorderItems(projectId, queueId, itemIds);
    } catch (err) {
      setSnackbar({ message: 'Failed to reorder items', severity: 'error' });
    }
  };

  const handleStatusChange = async (itemId: string, status: WorkQueueItemStatus) => {
    if (!projectId || !queueId) return;
    try {
      await updateItemStatus(projectId, queueId, itemId, status);
    } catch (err) {
      setSnackbar({ message: 'Failed to update status', severity: 'error' });
    }
  };

  const handleContextUpdate = async (request: UpdateContextRequest) => {
    if (!projectId || !queueId) return;
    try {
      await updateContext(projectId, queueId, request);
      setSnackbar({ message: 'Context updated', severity: 'success' });
    } catch (err) {
      setSnackbar({ message: 'Failed to update context', severity: 'error' });
      throw err;
    }
  };

  const handleDelete = async () => {
    if (!projectId || !queueId) return;
    setMenuAnchor(null);
    try {
      await deleteQueue(projectId, queueId);
      navigate(`/projects/${projectId}/queues`);
    } catch (err) {
      setSnackbar({ message: 'Failed to delete queue', severity: 'error' });
    }
  };

  const handleFetchTier = async (tier: number): Promise<TieredContextResponse> => {
    if (!projectId || !queueId) throw new Error('Missing project or queue ID');
    return await workQueuesApi.getTieredContext(projectId, queueId, tier);
  };

  const handleViewModeChange = (_event: React.MouseEvent<HTMLElement>, newMode: 'simple' | 'tiered' | null) => {
    if (newMode !== null) {
      setContextViewMode(newMode);
    }
  };

  if (!projectId || !queueId) {
    return <Alert severity="error">Project ID and Queue ID are required.</Alert>;
  }

  if (isLoading && !currentQueue) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={40} />
        <Skeleton variant="rectangular" height={200} sx={{ mt: 2 }} />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" onClose={clearError}>
        {error}
      </Alert>
    );
  }

  if (!currentQueue) {
    return <Alert severity="warning">Queue not found.</Alert>;
  }

  const completedItems = currentQueue.items.filter((i) => i.status === 'Completed').length;
  const totalItems = currentQueue.items.length;
  const progress = totalItems > 0 ? (completedItems / totalItems) * 100 : 0;

  const isCheckedOut = !!currentQueue.checkedOutBy;

  // Calculate staleness (24 hours)
  const lastUpdated = new Date(currentQueue.context.lastUpdated);
  const now = new Date();
  const hoursSinceUpdate = (now.getTime() - lastUpdated.getTime()) / (1000 * 60 * 60);
  const isStale = hoursSinceUpdate > 24;
  const staleWarning = isStale
    ? `Context was last updated ${Math.floor(hoursSinceUpdate)} hours ago. Consider reviewing and updating.`
    : null;

  return (
    <Box>
      {/* Breadcrumbs */}
      <Breadcrumbs separator={<NavigateNextIcon fontSize="small" />} sx={{ mb: 2 }}>
        <Link component={RouterLink} to="/projects" underline="hover" color="inherit">
          Projects
        </Link>
        <Link component={RouterLink} to={`/projects/${projectId}`} underline="hover" color="inherit">
          {currentQueue.projectName || projectId}
        </Link>
        <Link component={RouterLink} to={`/projects/${projectId}/queues`} underline="hover" color="inherit">
          Queues
        </Link>
        <Typography color="text.primary">{currentQueue.name}</Typography>
      </Breadcrumbs>

      {/* Checkout Banner */}
      {isCheckedOut && (
        <Alert
          severity="info"
          icon={<LockIcon />}
          action={
            <Button color="inherit" size="small" onClick={handleRelease} startIcon={<UnlockIcon />}>
              Release
            </Button>
          }
          sx={{ mb: 2 }}
        >
          Checked out by {currentQueue.checkedOutBy}
          {currentQueue.checkoutExpiresAt && ` until ${new Date(currentQueue.checkoutExpiresAt).toLocaleTimeString()}`}
        </Alert>
      )}

      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="h4">{currentQueue.name}</Typography>
            <QueueStatusChip status={currentQueue.status} size="medium" />
          </Box>
          {currentQueue.description && (
            <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
              {currentQueue.description}
            </Typography>
          )}
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {!isCheckedOut && (
            <Button
              variant="contained"
              startIcon={<LockIcon />}
              onClick={handleOpenCheckoutDialog}
            >
              Checkout
            </Button>
          )}
          <IconButton onClick={(e) => setMenuAnchor(e.currentTarget)}>
            <MoreIcon />
          </IconButton>
          <Menu
            anchorEl={menuAnchor}
            open={Boolean(menuAnchor)}
            onClose={() => setMenuAnchor(null)}
          >
            <MenuItem onClick={() => setMenuAnchor(null)}>
              <ArchiveIcon sx={{ mr: 1 }} /> Archive Queue
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleDelete} sx={{ color: 'error.main' }}>
              <DeleteIcon sx={{ mr: 1 }} /> Delete Queue
            </MenuItem>
          </Menu>
        </Box>
      </Box>

      {/* Progress */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2">Progress</Typography>
            <Typography variant="body2" color="text.secondary">
              {completedItems} / {totalItems} items completed
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={progress}
            sx={{ height: 8, borderRadius: 1 }}
          />
        </CardContent>
      </Card>

      {/* View Mode Toggle */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
        <ToggleButtonGroup
          value={contextViewMode}
          exclusive
          onChange={handleViewModeChange}
          size="small"
        >
          <ToggleButton value="simple">
            <Tooltip title="Simple Context View">
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <SimpleViewIcon fontSize="small" />
                <Typography variant="caption">Simple</Typography>
              </Box>
            </Tooltip>
          </ToggleButton>
          <ToggleButton value="tiered">
            <Tooltip title="Tiered Context View (for AI)">
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <TieredViewIcon fontSize="small" />
                <Typography variant="caption">Tiered</Typography>
              </Box>
            </Tooltip>
          </ToggleButton>
        </ToggleButtonGroup>
      </Box>

      {/* Two Column Layout */}
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3 }}>
        {/* Queue Items */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Queue Items
            </Typography>
            <DraggableQueueItemList
              items={currentQueue.items}
              onReorder={handleReorder}
              onStatusChange={handleStatusChange}
              disabled={!isCheckedOut}
            />
            {!isCheckedOut && currentQueue.items.length > 0 && (
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
                Checkout the queue to reorder items or change status.
              </Typography>
            )}
          </CardContent>
        </Card>

        {/* Context View - Simple or Tiered */}
        {contextViewMode === 'simple' ? (
          <ContextHelperPanel
            context={currentQueue.context}
            onUpdate={handleContextUpdate}
            isLoading={isLoading}
            isStale={isStale}
            staleWarning={staleWarning}
          />
        ) : (
          <TieredContextView
            projectId={projectId}
            queueId={queueId}
            onFetchTier={handleFetchTier}
          />
        )}
      </Box>

      {/* Snackbar */}
      <Snackbar
        open={!!snackbar}
        autoHideDuration={4000}
        onClose={() => setSnackbar(null)}
        message={snackbar?.message}
      />

      {/* Checkout Dialog */}
      <CheckoutDialog
        open={checkoutDialogOpen}
        onClose={() => setCheckoutDialogOpen(false)}
        onCheckout={handleCheckout}
        queueName={currentQueue.name}
        isConflict={!!checkoutConflict}
        conflictInfo={checkoutConflict || undefined}
      />
    </Box>
  );
}
