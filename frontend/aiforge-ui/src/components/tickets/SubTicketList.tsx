import { useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Collapse,
  IconButton,
  List,
  Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { SubTicketSummary, Ticket } from '../../types';
import SubTicketProgress from './SubTicketProgress';
import SubTicketItem from './SubTicketItem';
import CreateSubTicketForm from './CreateSubTicketForm';

interface SubTicketListProps {
  parentTicketId: string;
  subTickets: SubTicketSummary[];
  subTicketCount: number;
  completedSubTicketCount: number;
  subTicketProgress: number;
  onSubTicketCreated: (ticket: Ticket) => void;
  onSubTicketDeleted: (id: string) => void;
  onNavigate: (ticketId: string) => void;
}

export default function SubTicketList({
  parentTicketId,
  subTickets,
  subTicketCount,
  completedSubTicketCount,
  subTicketProgress,
  onSubTicketCreated,
  onSubTicketDeleted,
  onNavigate
}: SubTicketListProps) {
  const [expanded, setExpanded] = useState(true);
  const [showCreateForm, setShowCreateForm] = useState(false);

  return (
    <Card variant="outlined">
      <CardHeader
        title={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="h6">Sub-tickets</Typography>
            <Chip label={subTicketCount} size="small" />
          </Box>
        }
        action={
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => setShowCreateForm(true)}
            >
              Add
            </Button>
            <IconButton onClick={() => setExpanded(!expanded)}>
              {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            </IconButton>
          </Box>
        }
      />

      {subTicketCount > 0 && (
        <Box sx={{ px: 2, pb: 1 }}>
          <SubTicketProgress
            total={subTicketCount}
            completed={completedSubTicketCount}
            progress={subTicketProgress}
          />
        </Box>
      )}

      <Collapse in={expanded}>
        <CardContent sx={{ pt: 0 }}>
          {showCreateForm && (
            <Box sx={{ mb: 2 }}>
              <CreateSubTicketForm
                parentTicketId={parentTicketId}
                onCreated={(ticket) => {
                  onSubTicketCreated(ticket);
                  setShowCreateForm(false);
                }}
                onCancel={() => setShowCreateForm(false)}
              />
            </Box>
          )}

          {subTickets.length === 0 && !showCreateForm ? (
            <Typography color="text.secondary" align="center" sx={{ py: 2 }}>
              No sub-tickets yet. Click "Add" to break down this ticket.
            </Typography>
          ) : (
            <List dense>
              {subTickets.map((subTicket) => (
                <SubTicketItem
                  key={subTicket.id}
                  subTicket={subTicket}
                  onClick={() => onNavigate(subTicket.id)}
                  onDelete={onSubTicketDeleted}
                />
              ))}
            </List>
          )}
        </CardContent>
      </Collapse>
    </Card>
  );
}
