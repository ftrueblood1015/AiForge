import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField
} from '@mui/material';
import type { Ticket, TicketType, Priority, CreateSubTicketRequest } from '../../types';
import { ticketsApi } from '../../api/tickets';

interface CreateSubTicketFormProps {
  parentTicketId: string;
  onCreated: (ticket: Ticket) => void;
  onCancel: () => void;
}

export default function CreateSubTicketForm({
  parentTicketId,
  onCreated,
  onCancel
}: CreateSubTicketFormProps) {
  const [title, setTitle] = useState('');
  const [type, setType] = useState<TicketType>('Task');
  const [priority, setPriority] = useState<Priority>('Medium');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;

    setLoading(true);
    setError(null);

    try {
      const request: CreateSubTicketRequest = {
        title: title.trim(),
        type,
        priority,
      };
      const ticket = await ticketsApi.createSubTicket(parentTicketId, request);
      onCreated(ticket);
      setTitle('');
    } catch (err) {
      setError('Failed to create sub-ticket');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
      <Stack spacing={2}>
        <TextField
          label="Sub-ticket title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          size="small"
          fullWidth
          autoFocus
          disabled={loading}
        />
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Type</InputLabel>
            <Select
              value={type}
              label="Type"
              onChange={(e) => setType(e.target.value as TicketType)}
              disabled={loading}
            >
              <MenuItem value="Task">Task</MenuItem>
              <MenuItem value="Bug">Bug</MenuItem>
              <MenuItem value="Enhancement">Enhancement</MenuItem>
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Priority</InputLabel>
            <Select
              value={priority}
              label="Priority"
              onChange={(e) => setPriority(e.target.value as Priority)}
              disabled={loading}
            >
              <MenuItem value="Low">Low</MenuItem>
              <MenuItem value="Medium">Medium</MenuItem>
              <MenuItem value="High">High</MenuItem>
              <MenuItem value="Critical">Critical</MenuItem>
            </Select>
          </FormControl>
        </Box>
        {error && <Alert severity="error">{error}</Alert>}
        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
          <Button onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={loading || !title.trim()}
          >
            {loading ? <CircularProgress size={20} /> : 'Create'}
          </Button>
        </Box>
      </Stack>
    </Box>
  );
}
