import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  List,
  Skeleton,
  Alert,
  Button,
  Fab,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useQueueStore } from '../stores/queueStore';
import { QueueFilters, QueueListItem, CreateQueueDialog } from '../components/queues';
import type { WorkQueueStatus } from '../types';

export default function QueueList() {
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const {
    queues,
    isLoading,
    error,
    statusFilter,
    fetchQueues,
    createQueue,
    setStatusFilter,
    clearError,
  } = useQueueStore();

  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  useEffect(() => {
    if (projectId) {
      fetchQueues(projectId, statusFilter || undefined);
    }
  }, [projectId, statusFilter, fetchQueues]);

  const handleStatusChange = (status: WorkQueueStatus | null) => {
    setStatusFilter(status);
  };

  const handleClearFilters = () => {
    setStatusFilter(null);
  };

  const handleCreateQueue = async (name: string, description?: string) => {
    if (!projectId) return;
    setIsCreating(true);
    try {
      const queue = await createQueue(projectId, { name, description });
      setCreateDialogOpen(false);
      // Navigate to the new queue
      navigate(`/projects/${projectId}/queues/${queue.id}`);
    } finally {
      setIsCreating(false);
    }
  };

  const handleQueueClick = (queueId: string) => {
    navigate(`/projects/${projectId}/queues/${queueId}`);
  };

  if (!projectId) {
    return (
      <Alert severity="error">
        Project ID is required to view queues.
      </Alert>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Work Queues</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setCreateDialogOpen(true)}
        >
          Create Queue
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      <QueueFilters
        status={statusFilter}
        onStatusChange={handleStatusChange}
        onClear={handleClearFilters}
      />

      <Card>
        <CardContent>
          {isLoading ? (
            <Box>
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} variant="rectangular" height={72} sx={{ mb: 1, borderRadius: 1 }} />
              ))}
            </Box>
          ) : queues.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 6 }}>
              <Typography color="text.secondary" gutterBottom>
                {statusFilter ? 'No queues match your filter' : 'No work queues yet'}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {statusFilter
                  ? 'Try changing the status filter'
                  : 'Create a queue to organize your work items'}
              </Typography>
              {statusFilter ? (
                <Button onClick={handleClearFilters}>Clear Filter</Button>
              ) : (
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => setCreateDialogOpen(true)}
                >
                  Create Your First Queue
                </Button>
              )}
            </Box>
          ) : (
            <List disablePadding>
              {queues.map((queue) => (
                <QueueListItem
                  key={queue.id}
                  queue={queue}
                  onClick={() => handleQueueClick(queue.id)}
                />
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Results count */}
      {!isLoading && queues.length > 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Showing {queues.length} queue{queues.length !== 1 ? 's' : ''}
          {statusFilter && ` with status "${statusFilter}"`}
        </Typography>
      )}

      {/* FAB for mobile */}
      <Fab
        color="primary"
        aria-label="create queue"
        onClick={() => setCreateDialogOpen(true)}
        sx={{
          position: 'fixed',
          bottom: 16,
          right: 16,
          display: { xs: 'flex', sm: 'none' },
        }}
      >
        <AddIcon />
      </Fab>

      <CreateQueueDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSubmit={handleCreateQueue}
        isLoading={isCreating}
      />
    </Box>
  );
}
