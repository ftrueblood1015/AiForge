import React, { useEffect, useState } from 'react';
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
  Button,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useTicketStore } from '../stores/ticketStore';
import { TicketFilters } from '../components/tickets';
import type { TicketType, TicketStatus, Priority, TicketSearchParams } from '../types';

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
  const { tickets, isLoading, error, fetchTickets, clearError, searchParams, setSearchParams } = useTicketStore();
  const [filters, setFilters] = useState<TicketSearchParams>(searchParams);

  useEffect(() => {
    fetchTickets(filters);
  }, [fetchTickets, filters]);

  const handleFilterChange = (newFilters: TicketSearchParams) => {
    setFilters(newFilters);
    setSearchParams(newFilters);
  };

  const handleClearFilters = () => {
    const cleared: TicketSearchParams = {};
    setFilters(cleared);
    setSearchParams(cleared);
  };

  // Client-side filtering for search (API might not support all filters)
  const filteredTickets = tickets.filter((ticket) => {
    if (filters.search) {
      const search = filters.search.toLowerCase();
      if (
        !ticket.title.toLowerCase().includes(search) &&
        !ticket.key.toLowerCase().includes(search) &&
        !(ticket.description?.toLowerCase().includes(search))
      ) {
        return false;
      }
    }
    return true;
  });

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">All Tickets</Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      <TicketFilters
        filters={filters}
        onChange={handleFilterChange}
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
          ) : filteredTickets.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <Typography color="text.secondary" gutterBottom>
                {tickets.length === 0 ? 'No tickets found' : 'No tickets match your filters'}
              </Typography>
              {Object.keys(filters).length > 0 && (
                <Button onClick={handleClearFilters} sx={{ mt: 1 }}>
                  Clear Filters
                </Button>
              )}
            </Box>
          ) : (
            <List disablePadding>
              {filteredTickets.map((ticket) => (
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
                        <Typography variant="body2" color="text.secondary" fontWeight={500}>
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
                    <Chip label={ticket.status.replace(/([A-Z])/g, ' $1').trim()} size="small" color={statusColors[ticket.status]} />
                    <Chip label={ticket.priority} size="small" color={priorityColors[ticket.priority]} />
                  </Box>
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Results count */}
      {!isLoading && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Showing {filteredTickets.length} of {tickets.length} tickets
        </Typography>
      )}
    </Box>
  );
}
