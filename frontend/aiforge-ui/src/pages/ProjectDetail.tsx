import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Chip,
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
  Grid,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material';
import {
  Add as AddIcon,
  ArrowBack as ArrowBackIcon,
  ViewKanban as KanbanIcon,
  ViewList as ListIcon,
  Queue as QueueIcon,
} from '@mui/icons-material';
import { useProjectStore } from '../stores/projectStore';
import { useTicketStore } from '../stores/ticketStore';
import { TicketBoard } from '../components/tickets';
import type { Ticket, TicketType, TicketStatus, Priority } from '../types';

export default function ProjectDetail() {
  const { key } = useParams<{ key: string }>();
  const navigate = useNavigate();
  const { currentProject, isLoading: projectLoading, error: projectError, fetchProject } = useProjectStore();
  const { tickets, isLoading: ticketsLoading, error: ticketsError, fetchTickets, createTicket, updateTicketStatus } = useTicketStore();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [viewMode, setViewMode] = useState<'kanban' | 'list'>('kanban');
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

  const handleTicketClick = (ticket: Ticket) => {
    navigate(`/tickets/${ticket.key}`);
  };

  const handleStatusChange = async (ticketId: string, newStatus: TicketStatus) => {
    await updateTicketStatus(ticketId, newStatus);
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
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <ToggleButtonGroup
            value={viewMode}
            exclusive
            onChange={(_, value) => value && setViewMode(value)}
            size="small"
          >
            <ToggleButton value="kanban">
              <KanbanIcon sx={{ mr: 0.5 }} fontSize="small" />
              Kanban
            </ToggleButton>
            <ToggleButton value="list">
              <ListIcon sx={{ mr: 0.5 }} fontSize="small" />
              List
            </ToggleButton>
          </ToggleButtonGroup>
          <Button
            variant="outlined"
            startIcon={<QueueIcon />}
            onClick={() => navigate(`/projects/${currentProject.id}/queues`)}
          >
            Queues
          </Button>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
            New Ticket
          </Button>
        </Box>
      </Box>

      {/* Ticket Board / List */}
      {viewMode === 'kanban' ? (
        <TicketBoard
          tickets={tickets}
          isLoading={ticketsLoading}
          onTicketClick={handleTicketClick}
          onStatusChange={handleStatusChange}
        />
      ) : (
        <Box>
          {/* Simple list view - could be enhanced later */}
          {tickets.map((ticket) => (
            <Box
              key={ticket.id}
              onClick={() => handleTicketClick(ticket)}
              sx={{
                p: 2,
                mb: 1,
                backgroundColor: 'background.paper',
                borderRadius: 1,
                cursor: 'pointer',
                '&:hover': { boxShadow: 2 },
              }}
            >
              <Typography variant="body1" fontWeight={500}>
                {ticket.key} - {ticket.title}
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                <Chip label={ticket.status} size="small" />
                <Chip label={ticket.type} size="small" variant="outlined" />
                <Chip label={ticket.priority} size="small" />
              </Box>
            </Box>
          ))}
        </Box>
      )}

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
