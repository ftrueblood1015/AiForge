import React, { useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Skeleton,
  Alert,
} from '@mui/material';
import {
  Folder as FolderIcon,
  Assignment as AssignmentIcon,
  CheckCircle as CheckCircleIcon,
  PlayArrow as PlayArrowIcon,
  RateReview as RateReviewIcon,
  RadioButtonUnchecked as TodoIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useProjectStore } from '../stores/projectStore';
import { useTicketStore } from '../stores/ticketStore';
import type { TicketStatus } from '../types';

const statusIcons: Record<TicketStatus, React.ReactNode> = {
  ToDo: <TodoIcon color="action" />,
  InProgress: <PlayArrowIcon color="primary" />,
  InReview: <RateReviewIcon color="warning" />,
  Done: <CheckCircleIcon color="success" />,
};

const statusColors: Record<TicketStatus, 'default' | 'primary' | 'warning' | 'success'> = {
  ToDo: 'default',
  InProgress: 'primary',
  InReview: 'warning',
  Done: 'success',
};

export default function Dashboard() {
  const navigate = useNavigate();
  const { projects, isLoading: projectsLoading, error: projectsError, fetchProjects } = useProjectStore();
  const { tickets, isLoading: ticketsLoading, error: ticketsError, fetchTickets } = useTicketStore();

  useEffect(() => {
    fetchProjects();
    fetchTickets();
  }, [fetchProjects, fetchTickets]);

  const isLoading = projectsLoading || ticketsLoading;
  const error = projectsError || ticketsError;

  const ticketsByStatus = tickets.reduce(
    (acc, ticket) => {
      acc[ticket.status] = (acc[ticket.status] || 0) + 1;
      return acc;
    },
    {} as Record<TicketStatus, number>
  );

  const recentTickets = [...tickets]
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
    .slice(0, 5);

  if (error) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom>
          Dashboard
        </Typography>
        <Alert severity="error" sx={{ mt: 2 }}>
          {error}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      <Grid container spacing={3}>
        {/* Stats Cards */}
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FolderIcon color="primary" />
                <Typography color="text.secondary" variant="body2">
                  Projects
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{projects.length}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <AssignmentIcon color="primary" />
                <Typography color="text.secondary" variant="body2">
                  Total Tickets
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{tickets.length}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <PlayArrowIcon color="primary" />
                <Typography color="text.secondary" variant="body2">
                  In Progress
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{ticketsByStatus.InProgress || 0}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <CheckCircleIcon color="success" />
                <Typography color="text.secondary" variant="body2">
                  Completed
                </Typography>
              </Box>
              {isLoading ? (
                <Skeleton variant="text" width={60} height={40} />
              ) : (
                <Typography variant="h4">{ticketsByStatus.Done || 0}</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Tickets */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Tickets
              </Typography>
              {isLoading ? (
                <Box>
                  {[1, 2, 3].map((i) => (
                    <Skeleton key={i} variant="rectangular" height={60} sx={{ mb: 1, borderRadius: 1 }} />
                  ))}
                </Box>
              ) : recentTickets.length === 0 ? (
                <Typography color="text.secondary">No tickets yet</Typography>
              ) : (
                <List disablePadding>
                  {recentTickets.map((ticket) => (
                    <ListItem
                      key={ticket.id}
                      sx={{
                        cursor: 'pointer',
                        borderRadius: 1,
                        '&:hover': { backgroundColor: 'action.hover' },
                      }}
                      onClick={() => navigate(`/tickets/${ticket.key}`)}
                    >
                      <ListItemIcon>{statusIcons[ticket.status]}</ListItemIcon>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="body2" color="text.secondary">
                              {ticket.key}
                            </Typography>
                            <Typography variant="body1">{ticket.title}</Typography>
                          </Box>
                        }
                        secondary={new Date(ticket.updatedAt).toLocaleDateString()}
                      />
                      <Chip
                        label={ticket.status}
                        size="small"
                        color={statusColors[ticket.status]}
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Projects Overview */}
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Projects
              </Typography>
              {isLoading ? (
                <Box>
                  {[1, 2, 3].map((i) => (
                    <Skeleton key={i} variant="rectangular" height={60} sx={{ mb: 1, borderRadius: 1 }} />
                  ))}
                </Box>
              ) : projects.length === 0 ? (
                <Typography color="text.secondary">No projects yet</Typography>
              ) : (
                <List disablePadding>
                  {projects.map((project) => (
                    <ListItem
                      key={project.id}
                      sx={{
                        cursor: 'pointer',
                        borderRadius: 1,
                        '&:hover': { backgroundColor: 'action.hover' },
                      }}
                      onClick={() => navigate(`/projects/${project.key}`)}
                    >
                      <ListItemIcon>
                        <FolderIcon color="primary" />
                      </ListItemIcon>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="body2" color="text.secondary">
                              {project.key}
                            </Typography>
                            <Typography variant="body1">{project.name}</Typography>
                          </Box>
                        }
                        secondary={project.description || 'No description'}
                      />
                      <Chip
                        label={`${project.ticketCount} tickets`}
                        size="small"
                        variant="outlined"
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
