import React, { useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Skeleton,
  Alert,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
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

export default function TicketList() {
  const navigate = useNavigate();
  const { tickets, isLoading, error, fetchTickets, clearError } = useTicketStore();

  useEffect(() => {
    fetchTickets();
  }, [fetchTickets]);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        All Tickets
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      <Card>
        <CardContent>
          {isLoading ? (
            <Box>
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} variant="rectangular" height={72} sx={{ mb: 1, borderRadius: 1 }} />
              ))}
            </Box>
          ) : tickets.length === 0 ? (
            <Typography color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
              No tickets found
            </Typography>
          ) : (
            <List disablePadding>
              {tickets.map((ticket) => (
                <ListItem
                  key={ticket.id}
                  sx={{
                    borderRadius: 1,
                    mb: 1,
                    cursor: 'pointer',
                    '&:hover': { backgroundColor: 'action.hover' },
                  }}
                  onClick={() => navigate(`/tickets/${ticket.key}`)}
                >
                  <ListItemIcon>{typeIcons[ticket.type]}</ListItemIcon>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                          {ticket.key}
                        </Typography>
                        <Typography variant="body1">{ticket.title}</Typography>
                      </Box>
                    }
                    secondary={
                      <Typography variant="caption" color="text.secondary">
                        Updated {new Date(ticket.updatedAt).toLocaleString()}
                      </Typography>
                    }
                  />
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Chip label={ticket.status} size="small" color={statusColors[ticket.status]} />
                    <Chip label={ticket.priority} size="small" color={priorityColors[ticket.priority]} />
                  </Box>
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>
    </Box>
  );
}
