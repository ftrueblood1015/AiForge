import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Grid,
  Alert,
} from '@mui/material';
import type { Ticket, TicketType, Priority } from '../../types';

interface TicketFormData {
  title: string;
  description: string;
  type: TicketType;
  priority: Priority;
}

interface TicketFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: TicketFormData) => Promise<void>;
  ticket?: Ticket; // If provided, we're editing
  isLoading?: boolean;
  error?: string | null;
}

const initialFormData: TicketFormData = {
  title: '',
  description: '',
  type: 'Task',
  priority: 'Medium',
};

export default function TicketForm({
  open,
  onClose,
  onSubmit,
  ticket,
  isLoading = false,
  error = null,
}: TicketFormProps) {
  const [formData, setFormData] = useState<TicketFormData>(initialFormData);
  const [validationError, setValidationError] = useState<string | null>(null);

  const isEditing = !!ticket;

  useEffect(() => {
    if (ticket) {
      setFormData({
        title: ticket.title,
        description: ticket.description || '',
        type: ticket.type,
        priority: ticket.priority,
      });
    } else {
      setFormData(initialFormData);
    }
    setValidationError(null);
  }, [ticket, open]);

  const handleChange = (field: keyof TicketFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }));
    if (validationError) setValidationError(null);
  };

  const handleSubmit = async () => {
    if (!formData.title.trim()) {
      setValidationError('Title is required');
      return;
    }

    try {
      await onSubmit(formData);
      setFormData(initialFormData);
    } catch {
      // Error handled by parent
    }
  };

  const handleClose = () => {
    setFormData(initialFormData);
    setValidationError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEditing ? 'Edit Ticket' : 'Create New Ticket'}</DialogTitle>
      <DialogContent>
        {(error || validationError) && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error || validationError}
          </Alert>
        )}

        <TextField
          autoFocus
          margin="dense"
          label="Title"
          fullWidth
          required
          value={formData.title}
          onChange={handleChange('title')}
          error={!!validationError && !formData.title.trim()}
          sx={{ mb: 2 }}
        />

        <Grid container spacing={2} sx={{ mb: 2 }}>
          <Grid size={{ xs: 6 }}>
            <TextField
              select
              label="Type"
              fullWidth
              value={formData.type}
              onChange={handleChange('type')}
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
              value={formData.priority}
              onChange={handleChange('priority')}
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
          value={formData.description}
          onChange={handleChange('description')}
          placeholder="Add a detailed description..."
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isLoading || !formData.title.trim()}
        >
          {isLoading ? (isEditing ? 'Saving...' : 'Creating...') : (isEditing ? 'Save' : 'Create')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
