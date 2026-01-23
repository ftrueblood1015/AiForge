import {
  Box,
  TextField,
  MenuItem,
  Button,
} from '@mui/material';
import { Clear as ClearIcon } from '@mui/icons-material';
import type { WorkQueueStatus } from '../../types';

interface QueueFiltersProps {
  status: WorkQueueStatus | null;
  onStatusChange: (status: WorkQueueStatus | null) => void;
  onClear: () => void;
}

export default function QueueFilters({
  status,
  onStatusChange,
  onClear,
}: QueueFiltersProps) {
  return (
    <Box sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'center' }}>
      <TextField
        select
        label="Status"
        size="small"
        value={status || ''}
        onChange={(e) => onStatusChange((e.target.value as WorkQueueStatus) || null)}
        sx={{ minWidth: 160 }}
      >
        <MenuItem value="">All Statuses</MenuItem>
        <MenuItem value="Active">Active</MenuItem>
        <MenuItem value="Paused">Paused</MenuItem>
        <MenuItem value="Completed">Completed</MenuItem>
        <MenuItem value="Archived">Archived</MenuItem>
      </TextField>

      {status && (
        <Button
          startIcon={<ClearIcon />}
          onClick={onClear}
          size="small"
          color="inherit"
        >
          Clear Filter
        </Button>
      )}
    </Box>
  );
}
