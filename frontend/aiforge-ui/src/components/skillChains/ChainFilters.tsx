import {
  Box,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Switch,
} from '@mui/material';
import type { SkillChainSearchParams } from '../../types';

interface ChainFiltersProps {
  filters: SkillChainSearchParams;
  onChange: (filters: SkillChainSearchParams) => void;
  organizationId?: string;
  projectId?: string;
}

export default function ChainFilters({
  filters,
  onChange,
  organizationId,
  projectId,
}: ChainFiltersProps) {
  return (
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
      <FormControlLabel
        control={
          <Switch
            checked={filters.publishedOnly === true}
            onChange={(e) =>
              onChange({
                ...filters,
                publishedOnly: e.target.checked ? true : undefined,
              })
            }
          />
        }
        label="Published only"
      />
    </Box>
  );
}
