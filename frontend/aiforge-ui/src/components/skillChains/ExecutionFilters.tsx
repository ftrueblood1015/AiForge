import {
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import type { SkillChainExecutionSearchParams, ChainExecutionStatus } from '../../types';

interface ExecutionFiltersProps {
  filters: SkillChainExecutionSearchParams;
  onChange: (filters: SkillChainExecutionSearchParams) => void;
}

const statuses: ChainExecutionStatus[] = [
  'Pending',
  'Running',
  'Paused',
  'Completed',
  'Failed',
  'Cancelled',
];

export default function ExecutionFilters({
  filters,
  onChange,
}: ExecutionFiltersProps) {
  return (
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
      <FormControl size="small" sx={{ minWidth: 150 }}>
        <InputLabel>Status</InputLabel>
        <Select
          value={filters.status || ''}
          label="Status"
          onChange={(e) =>
            onChange({
              ...filters,
              status: e.target.value as ChainExecutionStatus || undefined,
            })
          }
        >
          <MenuItem value="">All</MenuItem>
          {statuses.map((status) => (
            <MenuItem key={status} value={status}>
              {status}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
    </Box>
  );
}
