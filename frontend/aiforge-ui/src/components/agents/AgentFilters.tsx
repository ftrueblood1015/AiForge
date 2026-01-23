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
import type { AgentSearchParams } from '../../types';

interface AgentFiltersProps {
  filters: AgentSearchParams;
  onChange: (filters: AgentSearchParams) => void;
  onClear: () => void;
  searchValue?: string;
  onSearchChange?: (search: string) => void;
}

export default function AgentFilters({
  filters,
  onChange,
  onClear,
  searchValue = '',
  onSearchChange,
}: AgentFiltersProps) {
  const handleChange = (field: keyof AgentSearchParams) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = e.target.value;
    onChange({
      ...filters,
      [field]: value || undefined,
    });
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (onSearchChange) {
      onSearchChange(e.target.value);
    }
  };

  const hasActiveFilters = !!(
    searchValue ||
    filters.agentType ||
    filters.status ||
    filters.isEnabled !== undefined
  );

  const activeFilterCount = [
    searchValue,
    filters.agentType,
    filters.status,
    filters.isEnabled !== undefined ? 'enabled' : undefined,
  ].filter(Boolean).length;

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        {/* Search */}
        <TextField
          placeholder="Search agents..."
          size="small"
          value={searchValue}
          onChange={handleSearchChange}
          sx={{ minWidth: 250 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" fontSize="small" />
              </InputAdornment>
            ),
          }}
        />

        {/* Agent Type Filter */}
        <TextField
          select
          label="Type"
          size="small"
          value={filters.agentType || ''}
          onChange={handleChange('agentType')}
          sx={{ minWidth: 130 }}
        >
          <MenuItem value="">All Types</MenuItem>
          <MenuItem value="Claude">Claude</MenuItem>
          <MenuItem value="GPT">GPT</MenuItem>
          <MenuItem value="Gemini">Gemini</MenuItem>
          <MenuItem value="Custom">Custom</MenuItem>
        </TextField>

        {/* Status Filter */}
        <TextField
          select
          label="Status"
          size="small"
          value={filters.status || ''}
          onChange={handleChange('status')}
          sx={{ minWidth: 130 }}
        >
          <MenuItem value="">All Statuses</MenuItem>
          <MenuItem value="Idle">Idle</MenuItem>
          <MenuItem value="Working">Working</MenuItem>
          <MenuItem value="Paused">Paused</MenuItem>
          <MenuItem value="Disabled">Disabled</MenuItem>
          <MenuItem value="Error">Error</MenuItem>
        </TextField>

        {/* Enabled Filter */}
        <TextField
          select
          label="Enabled"
          size="small"
          value={filters.isEnabled === undefined ? '' : filters.isEnabled ? 'true' : 'false'}
          onChange={(e) => {
            const value = e.target.value;
            onChange({
              ...filters,
              isEnabled: value === '' ? undefined : value === 'true',
            });
          }}
          sx={{ minWidth: 130 }}
        >
          <MenuItem value="">All</MenuItem>
          <MenuItem value="true">Enabled</MenuItem>
          <MenuItem value="false">Disabled</MenuItem>
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
          {searchValue && (
            <Chip
              label={`Search: "${searchValue}"`}
              size="small"
              onDelete={() => onSearchChange?.('')}
            />
          )}
          {filters.agentType && (
            <Chip
              label={`Type: ${filters.agentType}`}
              size="small"
              color="primary"
              onDelete={() => onChange({ ...filters, agentType: undefined })}
            />
          )}
          {filters.status && (
            <Chip
              label={`Status: ${filters.status}`}
              size="small"
              color="secondary"
              onDelete={() => onChange({ ...filters, status: undefined })}
            />
          )}
          {filters.isEnabled !== undefined && (
            <Chip
              label={`Enabled: ${filters.isEnabled ? 'Yes' : 'No'}`}
              size="small"
              color="info"
              onDelete={() => onChange({ ...filters, isEnabled: undefined })}
            />
          )}
        </Box>
      )}
    </Box>
  );
}
