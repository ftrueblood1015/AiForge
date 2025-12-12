import React from 'react';
import {
  Box,
  TextField,
  MenuItem,
  InputAdornment,
  Chip,
  Button,
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
} from '@mui/icons-material';
import type { TicketSearchParams } from '../../types';

interface TicketFiltersProps {
  filters: TicketSearchParams;
  onChange: (filters: TicketSearchParams) => void;
  onClear: () => void;
}

export default function TicketFilters({
  filters,
  onChange,
  onClear,
}: TicketFiltersProps) {
  const handleChange = (field: keyof TicketSearchParams) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = e.target.value;
    onChange({
      ...filters,
      [field]: value || undefined,
    });
  };

  const hasActiveFilters = !!(
    filters.search ||
    filters.status ||
    filters.type ||
    filters.priority
  );

  const activeFilterCount = [
    filters.search,
    filters.status,
    filters.type,
    filters.priority,
  ].filter(Boolean).length;

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        {/* Search */}
        <TextField
          placeholder="Search tickets..."
          size="small"
          value={filters.search || ''}
          onChange={handleChange('search')}
          sx={{ minWidth: 250 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" fontSize="small" />
              </InputAdornment>
            ),
          }}
        />

        {/* Status Filter */}
        <TextField
          select
          label="Status"
          size="small"
          value={filters.status || ''}
          onChange={handleChange('status')}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">All Statuses</MenuItem>
          <MenuItem value="ToDo">To Do</MenuItem>
          <MenuItem value="InProgress">In Progress</MenuItem>
          <MenuItem value="InReview">In Review</MenuItem>
          <MenuItem value="Done">Done</MenuItem>
        </TextField>

        {/* Type Filter */}
        <TextField
          select
          label="Type"
          size="small"
          value={filters.type || ''}
          onChange={handleChange('type')}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">All Types</MenuItem>
          <MenuItem value="Task">Task</MenuItem>
          <MenuItem value="Bug">Bug</MenuItem>
          <MenuItem value="Feature">Feature</MenuItem>
          <MenuItem value="Enhancement">Enhancement</MenuItem>
        </TextField>

        {/* Priority Filter */}
        <TextField
          select
          label="Priority"
          size="small"
          value={filters.priority || ''}
          onChange={handleChange('priority')}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">All Priorities</MenuItem>
          <MenuItem value="Low">Low</MenuItem>
          <MenuItem value="Medium">Medium</MenuItem>
          <MenuItem value="High">High</MenuItem>
          <MenuItem value="Critical">Critical</MenuItem>
        </TextField>

        {/* Clear Filters Button */}
        {hasActiveFilters && (
          <Button
            startIcon={<ClearIcon />}
            onClick={onClear}
            size="small"
            color="inherit"
          >
            Clear ({activeFilterCount})
          </Button>
        )}
      </Box>

      {/* Active Filters Display */}
      {hasActiveFilters && (
        <Box sx={{ display: 'flex', gap: 1, mt: 2, flexWrap: 'wrap' }}>
          {filters.search && (
            <Chip
              label={`Search: "${filters.search}"`}
              size="small"
              onDelete={() => onChange({ ...filters, search: undefined })}
            />
          )}
          {filters.status && (
            <Chip
              label={`Status: ${filters.status}`}
              size="small"
              color="primary"
              onDelete={() => onChange({ ...filters, status: undefined })}
            />
          )}
          {filters.type && (
            <Chip
              label={`Type: ${filters.type}`}
              size="small"
              color="secondary"
              onDelete={() => onChange({ ...filters, type: undefined })}
            />
          )}
          {filters.priority && (
            <Chip
              label={`Priority: ${filters.priority}`}
              size="small"
              color="warning"
              onDelete={() => onChange({ ...filters, priority: undefined })}
            />
          )}
        </Box>
      )}
    </Box>
  );
}
