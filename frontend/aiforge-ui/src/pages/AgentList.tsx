import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from '@mui/material';
import Grid from '@mui/material/Grid';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as EnableIcon,
  Stop as DisableIcon,
} from '@mui/icons-material';
import { useAgentStore } from '../stores/agentStore';
import { AgentCard, AgentFilters, AgentForm } from '../components/agents';
import { CardSkeleton } from '../components/common';
import type { AgentListItem, AgentSearchParams, CreateAgentRequest, UpdateAgentRequest } from '../types';

export default function AgentList() {
  const {
    agents,
    currentAgent,
    isLoading,
    error,
    fetchAgents,
    fetchAgent,
    createAgent,
    updateAgent,
    deleteAgent,
    enableAgent,
    disableAgent,
    clearError,
    searchParams,
    setSearchParams,
  } = useAgentStore();

  const [filters, setFilters] = useState<AgentSearchParams>(searchParams);
  const [searchValue, setSearchValue] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [editingAgent, setEditingAgent] = useState<AgentListItem | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [agentToDelete, setAgentToDelete] = useState<AgentListItem | null>(null);
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [menuAgent, setMenuAgent] = useState<AgentListItem | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchAgents(filters);
  }, [fetchAgents, filters]);

  const handleFilterChange = (newFilters: AgentSearchParams) => {
    setFilters(newFilters);
    setSearchParams(newFilters);
  };

  const handleClearFilters = () => {
    const cleared: AgentSearchParams = {};
    setFilters(cleared);
    setSearchParams(cleared);
    setSearchValue('');
  };

  // Client-side filtering for search
  const filteredAgents = agents.filter((agent) => {
    if (searchValue) {
      const search = searchValue.toLowerCase();
      if (
        !agent.name.toLowerCase().includes(search) &&
        !agent.agentKey.toLowerCase().includes(search) &&
        !(agent.description?.toLowerCase().includes(search))
      ) {
        return false;
      }
    }
    return true;
  });

  const handleMenuOpen = (event: React.MouseEvent, agent: AgentListItem) => {
    setMenuAnchor(event.currentTarget as HTMLElement);
    setMenuAgent(agent);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setMenuAgent(null);
  };

  const handleCreateClick = () => {
    setEditingAgent(null);
    setFormError(null);
    setFormOpen(true);
  };

  const handleEditClick = async () => {
    if (menuAgent) {
      await fetchAgent(menuAgent.id);
      setEditingAgent(menuAgent);
      setFormError(null);
      setFormOpen(true);
    }
    handleMenuClose();
  };

  const handleDeleteClick = () => {
    if (menuAgent) {
      setAgentToDelete(menuAgent);
      setDeleteConfirmOpen(true);
    }
    handleMenuClose();
  };

  const handleEnableToggle = async () => {
    if (menuAgent) {
      try {
        if (menuAgent.isEnabled) {
          await disableAgent(menuAgent.id);
        } else {
          await enableAgent(menuAgent.id);
        }
      } catch {
        // Error handled by store
      }
    }
    handleMenuClose();
  };

  const handleFormSubmit = async (data: CreateAgentRequest | UpdateAgentRequest) => {
    setIsSubmitting(true);
    setFormError(null);
    try {
      if (editingAgent) {
        await updateAgent(editingAgent.id, data as UpdateAgentRequest);
      } else {
        await createAgent(data as CreateAgentRequest);
      }
      setFormOpen(false);
      setEditingAgent(null);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'An error occurred');
      throw err;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleFormClose = () => {
    setFormOpen(false);
    setEditingAgent(null);
    setFormError(null);
  };

  const handleDeleteConfirm = async () => {
    if (agentToDelete) {
      try {
        await deleteAgent(agentToDelete.id);
        setDeleteConfirmOpen(false);
        setAgentToDelete(null);
      } catch {
        // Error handled by store
      }
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmOpen(false);
    setAgentToDelete(null);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Agents</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleCreateClick}
        >
          Create Agent
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      <AgentFilters
        filters={filters}
        onChange={handleFilterChange}
        onClear={handleClearFilters}
        searchValue={searchValue}
        onSearchChange={setSearchValue}
      />

      {isLoading ? (
        <Grid container spacing={2}>
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={i}>
              <CardSkeleton />
            </Grid>
          ))}
        </Grid>
      ) : filteredAgents.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 6, bgcolor: 'background.paper', borderRadius: 1 }}>
          <Typography color="text.secondary" gutterBottom>
            {agents.length === 0 ? 'No agents configured' : 'No agents match your filters'}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {agents.length === 0
              ? 'Create your first agent to get started with AI configuration.'
              : 'Try adjusting your search or filter criteria.'}
          </Typography>
          {agents.length === 0 ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateClick}>
              Create Agent
            </Button>
          ) : (
            <Button onClick={handleClearFilters}>Clear Filters</Button>
          )}
        </Box>
      ) : (
        <Grid container spacing={2}>
          {filteredAgents.map((agent) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={agent.id}>
              <AgentCard
                agent={agent}
                onMenuClick={handleMenuOpen}
                onToggleEnabled={async (a: AgentListItem, enabled: boolean) => {
                  try {
                    if (enabled) {
                      await enableAgent(a.id);
                    } else {
                      await disableAgent(a.id);
                    }
                  } catch {
                    // Error handled by store
                  }
                }}
              />
            </Grid>
          ))}
        </Grid>
      )}

      {/* Results count */}
      {!isLoading && filteredAgents.length > 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Showing {filteredAgents.length} of {agents.length} agents
        </Typography>
      )}

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={handleEditClick}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleEnableToggle}>
          <ListItemIcon>
            {menuAgent?.isEnabled ? (
              <DisableIcon fontSize="small" />
            ) : (
              <EnableIcon fontSize="small" />
            )}
          </ListItemIcon>
          <ListItemText>
            {menuAgent?.isEnabled ? 'Disable' : 'Enable'}
          </ListItemText>
        </MenuItem>
        <MenuItem onClick={handleDeleteClick} sx={{ color: 'error.main' }}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText>Delete</ListItemText>
        </MenuItem>
      </Menu>

      {/* Create/Edit Form Dialog */}
      <AgentForm
        open={formOpen}
        onClose={handleFormClose}
        onSubmit={handleFormSubmit}
        agent={editingAgent ? currentAgent || undefined : undefined}
        isLoading={isSubmitting}
        error={formError}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Delete Agent</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the agent "{agentToDelete?.name}"?
            This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDeleteCancel}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
