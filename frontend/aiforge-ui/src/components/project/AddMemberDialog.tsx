import {
  Autocomplete,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import { useCallback, useState } from 'react';
import type { ProjectRole, UserSearchResult } from '../../types';
import { projectMembersApi } from '../../api/projectMembers';

interface AddMemberDialogProps {
  open: boolean;
  projectId: string;
  onClose: () => void;
  onAdd: (email: string, role: ProjectRole) => Promise<void>;
}

export default function AddMemberDialog({
  open,
  projectId,
  onClose,
  onAdd,
}: AddMemberDialogProps) {
  const [selectedUser, setSelectedUser] = useState<UserSearchResult | null>(null);
  const [role, setRole] = useState<ProjectRole>('Member');
  const [searchQuery, setSearchQuery] = useState('');
  const [options, setOptions] = useState<UserSearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const searchUsers = useCallback(
    async (query: string) => {
      if (query.length < 2) {
        setOptions([]);
        return;
      }

      setLoading(true);
      try {
        const results = await projectMembersApi.searchUsers(query, projectId);
        setOptions(results);
      } catch (err) {
        console.error('Failed to search users:', err);
        setOptions([]);
      } finally {
        setLoading(false);
      }
    },
    [projectId]
  );

  const handleInputChange = (_event: React.SyntheticEvent, value: string) => {
    setSearchQuery(value);
    // Debounce search
    const timeoutId = setTimeout(() => {
      searchUsers(value);
    }, 300);
    return () => clearTimeout(timeoutId);
  };

  const handleSubmit = async () => {
    if (!selectedUser) return;

    setSubmitting(true);
    setError(null);
    try {
      await onAdd(selectedUser.email, role);
      handleClose();
    } catch (err) {
      setError((err as Error).message || 'Failed to add member');
    } finally {
      setSubmitting(false);
    }
  };

  const handleClose = () => {
    setSelectedUser(null);
    setRole('Member');
    setSearchQuery('');
    setOptions([]);
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Add Project Member</DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          <Autocomplete
            value={selectedUser}
            onChange={(_event, newValue) => setSelectedUser(newValue)}
            inputValue={searchQuery}
            onInputChange={handleInputChange}
            options={options}
            getOptionLabel={(option) => `${option.displayName} (${option.email})`}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            loading={loading}
            noOptionsText={
              searchQuery.length < 2
                ? 'Type at least 2 characters to search'
                : 'No users found'
            }
            renderInput={(params) => (
              <TextField
                {...params}
                label="Search users"
                placeholder="Search by name or email"
                InputProps={{
                  ...params.InputProps,
                  endAdornment: (
                    <>
                      {loading && <CircularProgress color="inherit" size={20} />}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
            renderOption={(props, option) => (
              <li {...props} key={option.id}>
                <Box>
                  <Typography variant="body1">{option.displayName}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {option.email}
                  </Typography>
                </Box>
              </li>
            )}
          />

          <FormControl fullWidth>
            <InputLabel>Role</InputLabel>
            <Select
              value={role}
              label="Role"
              onChange={(e) => setRole(e.target.value as ProjectRole)}
            >
              <MenuItem value="Viewer">Viewer - Read-only access</MenuItem>
              <MenuItem value="Member">Member - Can participate in tickets</MenuItem>
              <MenuItem value="Owner">Owner - Can manage project and members</MenuItem>
            </Select>
          </FormControl>

          {error && (
            <Typography color="error" variant="body2">
              {error}
            </Typography>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={submitting}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={!selectedUser || submitting}
        >
          {submitting ? 'Adding...' : 'Add Member'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
