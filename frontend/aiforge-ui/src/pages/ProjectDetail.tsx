import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Chip,
  Card,
  CardContent,
  Grid,
  Skeleton,
  Alert,
  Breadcrumbs,
  Link,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
} from '@mui/material';
import {
  Add as AddIcon,
  ArrowBack as ArrowBackIcon,
  Assignment as AssignmentIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
} from '@mui/icons-material';
import { useProjectStore } from '../stores/projectStore';
import { useTicketStore } from '../stores/ticketStore';
import type { TicketType, TicketStatus, Priority } from '../types';

const typeIcons: Record<TicketType, React.ReactNode> = {
  Task: <AssignmentIcon color="action" />,
  Bug: <BugIcon color="error" />,
  Feature: <FeatureIcon color="warning" />,
  Enhancement: <EnhancementIcon color="info" />,
};

const statusColors: Record<TicketStatus, 'default' | 'primary' | 'warning' | 'success'> = {
  ToDo: 'default',
  InProgress: 'primary',
  InReview: 'warning',
  Done: 'success',
};

const priorityColors: Record<Priority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

export default function ProjectDetail() {
  const { key } = useParams<{ key: string }>();
  const navigate = useNavigate();
  const { currentProject, isLoading: projectLoading, error: projectError, fetchProject } = useProjectStore();
  const { tickets, isLoading: ticketsLoading, error: ticketsError, fetchTickets, createTicket } = useTicketStore();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [newTicket, setNewTicket] = useState({
    title: '',
    description: '',
    type: 'Task' as TicketType,
    priority: 'Medium' as Priority,
  });
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    if (key) {
      fetchProject(key);
    }
  }, [key, fetchProject]);

  useEffect(() => {
    if (currentProject) {
      fetchTickets({ projectId: currentProject.id });
    }
  }, [currentProject, fetchTickets]);

  const isLoading = projectLoading || ticketsLoading;
  const error = projectError || ticketsError;

  const handleCreateTicket = async () => {
    if (!newTicket.title || !currentProject) return;

    setCreating(true);
    try {
      await createTicket(
        currentProject.id,
        newTicket.title,
        newTicket.type,
        newTicket.description || undefined,
        newTicket.priority
      );
      setDialogOpen(false);
      setNewTicket({ title: '', description: '', type: 'Task', priority: 'Medium' });
      fetchTickets({ projectId: currentProject.id });
    } catch {
      // Error handled in store
    } finally {
      setCreating(false);
    }
  };

  if (error) {
    return (
      <Box>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/projects')} sx={{ mb: 2 }}>
          Back to Projects
        </Button>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (isLoading && !currentProject) {
    return (
      <Box>
        <Skeleton variant="text" width={200} height={40} />
        <Skeleton variant="rectangular" height={200} sx={{ mt: 2, borderRadius: 2 }} />
      </Box>
    );
  }

  if (!currentProject) {
    return (
      <Box>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/projects')} sx={{ mb: 2 }}>
          Back to Projects
        </Button>
        <Alert severity="warning">Project not found</Alert>
      </Box>
    );
  }

  // Group tickets by status for Kanban-like view
  const ticketsByStatus: Record<TicketStatus, typeof tickets> = {
    ToDo: tickets.filter((t) => t.status === 'ToDo'),
    InProgress: tickets.filter((t) => t.status === 'InProgress'),
    InReview: tickets.filter((t) => t.status === 'InReview'),
    Done: tickets.filter((t) => t.status === 'Done'),
  };

  return (
    <Box>
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          component="button"
          variant="body2"
          onClick={() => navigate('/projects')}
          underline="hover"
          color="inherit"
        >
          Projects
        </Link>
        <Typography color="text.primary">{currentProject.key}</Typography>
      </Breadcrumbs>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
            <Typography variant="h4">{currentProject.name}</Typography>
            <Chip label={currentProject.key} color="primary" />
          </Box>
          <Typography variant="body1" color="text.secondary">
            {currentProject.description || 'No description'}
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
          New Ticket
        </Button>
      </Box>

      {/* Kanban Board */}
      <Grid container spacing={2}>
        {(['ToDo', 'InProgress', 'InReview', 'Done'] as TicketStatus[]).map((status) => (
          <Grid size={{ xs: 12, sm: 6, md: 3 }} key={status}>
            <Card sx={{ backgroundColor: 'background.default' }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="subtitle1" fontWeight={600}>
                    {status.replace(/([A-Z])/g, ' $1').trim()}
                  </Typography>
                  <Chip
                    label={ticketsByStatus[status].length}
                    size="small"
                    color={statusColors[status]}
                  />
                </Box>

                {ticketsLoading ? (
                  <Box>
                    {[1, 2].map((i) => (
                      <Skeleton key={i} variant="rectangular" height={80} sx={{ mb: 1, borderRadius: 1 }} />
                    ))}
                  </Box>
                ) : ticketsByStatus[status].length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
                    No tickets
                  </Typography>
                ) : (
                  <List disablePadding>
                    {ticketsByStatus[status].map((ticket) => (
                      <ListItem
                        key={ticket.id}
                        sx={{
                          backgroundColor: 'background.paper',
                          borderRadius: 1,
                          mb: 1,
                          cursor: 'pointer',
                          '&:hover': { boxShadow: 1 },
                        }}
                        onClick={() => navigate(`/tickets/${ticket.key}`)}
                      >
                        <ListItemIcon sx={{ minWidth: 36 }}>
                          {typeIcons[ticket.type]}
                        </ListItemIcon>
                        <ListItemText
                          primary={
                            <Typography variant="body2" fontWeight={500}>
                              {ticket.title}
                            </Typography>
                          }
                          secondary={
                            <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                              <Chip label={ticket.key} size="small" variant="outlined" />
                              <Chip
                                label={ticket.priority}
                                size="small"
                                color={priorityColors[ticket.priority]}
                              />
                            </Box>
                          }
                        />
                      </ListItem>
                    ))}
                  </List>
                )}
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Create Ticket Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create New Ticket</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Title"
            fullWidth
            value={newTicket.title}
            onChange={(e) => setNewTicket({ ...newTicket, title: e.target.value })}
            sx={{ mb: 2 }}
          />
          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid size={{ xs: 6 }}>
              <TextField
                select
                label="Type"
                fullWidth
                value={newTicket.type}
                onChange={(e) => setNewTicket({ ...newTicket, type: e.target.value as TicketType })}
              >
                <MenuItem value="Task">Task</MenuItem>
                <MenuItem value="Bug">Bug</MenuItem>
                <MenuItem value="Feature">Feature</MenuItem>
                <MenuItem value="Enhancement">Enhancement</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 6 }}>
              <TextField
                select
                label="Priority"
                fullWidth
                value={newTicket.priority}
                onChange={(e) => setNewTicket({ ...newTicket, priority: e.target.value as Priority })}
              >
                <MenuItem value="Low">Low</MenuItem>
                <MenuItem value="Medium">Medium</MenuItem>
                <MenuItem value="High">High</MenuItem>
                <MenuItem value="Critical">Critical</MenuItem>
              </TextField>
            </Grid>
          </Grid>
          <TextField
            margin="dense"
            label="Description"
            fullWidth
            multiline
            rows={4}
            value={newTicket.description}
            onChange={(e) => setNewTicket({ ...newTicket, description: e.target.value })}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleCreateTicket}
            variant="contained"
            disabled={!newTicket.title || creating}
          >
            {creating ? 'Creating...' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
