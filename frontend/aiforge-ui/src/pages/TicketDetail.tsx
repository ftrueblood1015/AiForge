import React, { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Chip,
  Card,
  CardContent,
  Grid,
  Skeleton,
  Alert,
  Breadcrumbs,
  Link,
  Divider,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
} from '@mui/icons-material';
import { useTicketStore } from '../stores/ticketStore';
import type { TicketType, TicketStatus, Priority } from '../types';

const typeIcons: Record<TicketType, React.ReactNode> = {
  Task: <AssignmentIcon color="action" fontSize="large" />,
  Bug: <BugIcon color="error" fontSize="large" />,
  Feature: <FeatureIcon color="warning" fontSize="large" />,
  Enhancement: <EnhancementIcon color="info" fontSize="large" />,
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

export default function TicketDetail() {
  const { key } = useParams<{ key: string }>();
  const navigate = useNavigate();
  const { currentTicket, isLoading, error, fetchTicket } = useTicketStore();

  useEffect(() => {
    if (key) {
      fetchTicket(key);
    }
  }, [key, fetchTicket]);

  if (error) {
    return (
      <Box>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (isLoading || !currentTicket) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={40} />
        <Skeleton variant="rectangular" height={400} sx={{ mt: 2, borderRadius: 2 }} />
      </Box>
    );
  }

  const projectKey = currentTicket.key.split('-')[0];

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
        <Link
          component="button"
          variant="body2"
          onClick={() => navigate(`/projects/${projectKey}`)}
          underline="hover"
          color="inherit"
        >
          {projectKey}
        </Link>
        <Typography color="text.primary">{currentTicket.key}</Typography>
      </Breadcrumbs>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                {typeIcons[currentTicket.type]}
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {currentTicket.key}
                  </Typography>
                  <Typography variant="h5">{currentTicket.title}</Typography>
                </Box>
              </Box>

              <Box sx={{ display: 'flex', gap: 1, mb: 3 }}>
                <Chip label={currentTicket.status} color={statusColors[currentTicket.status]} />
                <Chip label={currentTicket.type} variant="outlined" />
                <Chip label={currentTicket.priority} color={priorityColors[currentTicket.priority]} />
              </Box>

              <Divider sx={{ my: 2 }} />

              <Typography variant="h6" gutterBottom>
                Description
              </Typography>
              <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                {currentTicket.description || 'No description provided.'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Details
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Status
                  </Typography>
                  <Typography variant="body1">{currentTicket.status}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Type
                  </Typography>
                  <Typography variant="body1">{currentTicket.type}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Priority
                  </Typography>
                  <Typography variant="body1">{currentTicket.priority}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Created
                  </Typography>
                  <Typography variant="body1">
                    {new Date(currentTicket.createdAt).toLocaleString()}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Updated
                  </Typography>
                  <Typography variant="body1">
                    {new Date(currentTicket.updatedAt).toLocaleString()}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
