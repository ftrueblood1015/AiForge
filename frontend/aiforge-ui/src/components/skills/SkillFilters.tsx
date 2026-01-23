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
import type { SkillSearchParams } from '../../types';

interface SkillFiltersProps {
  filters: SkillSearchParams;
  onChange: (filters: SkillSearchParams) => void;
  onClear: () => void;
  searchValue?: string;
  onSearchChange?: (search: string) => void;
}

export default function SkillFilters({
  filters,
  onChange,
  onClear,
  searchValue = '',
  onSearchChange,
}: SkillFiltersProps) {
  const handleChange = (field: keyof SkillSearchParams) => (
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
    filters.category ||
    filters.isPublished !== undefined
  );

  const activeFilterCount = [
    searchValue,
    filters.category,
    filters.isPublished !== undefined ? 'published' : undefined,
  ].filter(Boolean).length;

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        {/* Search */}
        <TextField
          placeholder="Search skills..."
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

        {/* Category Filter */}
        <TextField
          select
          label="Category"
          size="small"
          value={filters.category || ''}
          onChange={handleChange('category')}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">All Categories</MenuItem>
          <MenuItem value="Workflow">Workflow</MenuItem>
          <MenuItem value="Analysis">Analysis</MenuItem>
          <MenuItem value="Documentation">Documentation</MenuItem>
          <MenuItem value="Generation">Generation</MenuItem>
          <MenuItem value="Testing">Testing</MenuItem>
          <MenuItem value="Custom">Custom</MenuItem>
        </TextField>

        {/* Published Filter */}
        <TextField
          select
          label="Status"
          size="small"
          value={filters.isPublished === undefined ? '' : filters.isPublished ? 'true' : 'false'}
          onChange={(e) => {
            const value = e.target.value;
            onChange({
              ...filters,
              isPublished: value === '' ? undefined : value === 'true',
            });
          }}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">All</MenuItem>
          <MenuItem value="true">Published</MenuItem>
          <MenuItem value="false">Unpublished</MenuItem>
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
          {filters.category && (
            <Chip
              label={`Category: ${filters.category}`}
              size="small"
              color="primary"
              onDelete={() => onChange({ ...filters, category: undefined })}
            />
          )}
          {filters.isPublished !== undefined && (
            <Chip
              label={`Status: ${filters.isPublished ? 'Published' : 'Unpublished'}`}
              size="small"
              color="secondary"
              onDelete={() => onChange({ ...filters, isPublished: undefined })}
            />
          )}
        </Box>
      )}
    </Box>
  );
}
