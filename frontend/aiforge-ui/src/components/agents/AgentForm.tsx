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
  Chip,
  Typography,
  Autocomplete,
} from '@mui/material';
import type { Agent, AgentType, CreateAgentRequest, UpdateAgentRequest } from '../../types';

interface AgentFormData {
  agentKey: string;
  name: string;
  description: string;
  agentType: AgentType;
  systemPrompt: string;
  instructions: string;
  capabilities: string[];
  organizationId?: string;
  projectId?: string;
}

interface AgentFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateAgentRequest | UpdateAgentRequest) => Promise<void>;
  agent?: Agent;
  isLoading?: boolean;
  error?: string | null;
  organizationId?: string;
  projectId?: string;
}

const AGENT_TYPES: AgentType[] = ['Claude', 'GPT', 'Gemini', 'Custom'];

const COMMON_CAPABILITIES = [
  'coding',
  'testing',
  'documentation',
  'review',
  'research',
  'planning',
  'debugging',
  'refactoring',
];

const initialFormData: AgentFormData = {
  agentKey: '',
  name: '',
  description: '',
  agentType: 'Claude',
  systemPrompt: '',
  instructions: '',
  capabilities: [],
};

export default function AgentForm({
  open,
  onClose,
  onSubmit,
  agent,
  isLoading = false,
  error = null,
  organizationId,
  projectId,
}: AgentFormProps) {
  const [formData, setFormData] = useState<AgentFormData>(initialFormData);
  const [validationError, setValidationError] = useState<string | null>(null);

  const isEditing = !!agent;

  useEffect(() => {
    if (agent) {
      setFormData({
        agentKey: agent.agentKey,
        name: agent.name,
        description: agent.description || '',
        agentType: agent.agentType,
        systemPrompt: agent.systemPrompt || '',
        instructions: agent.instructions || '',
        capabilities: agent.capabilities || [],
      });
    } else {
      setFormData({
        ...initialFormData,
        organizationId,
        projectId,
      });
    }
    setValidationError(null);
  }, [agent, open, organizationId, projectId]);

  const handleChange = (field: keyof AgentFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }));
    if (validationError) setValidationError(null);
  };

  const handleCapabilitiesChange = (_: unknown, newValue: string[]) => {
    setFormData((prev) => ({ ...prev, capabilities: newValue }));
  };

  const handleSubmit = async () => {
    if (!formData.name.trim()) {
      setValidationError('Name is required');
      return;
    }

    if (!isEditing && !formData.agentKey.trim()) {
      setValidationError('Agent Key is required');
      return;
    }

    if (!isEditing && !/^[a-z0-9-]+$/.test(formData.agentKey)) {
      setValidationError('Agent Key must be lowercase letters, numbers, and hyphens only');
      return;
    }

    try {
      if (isEditing) {
        const updateData: UpdateAgentRequest = {
          name: formData.name,
          description: formData.description || undefined,
          agentType: formData.agentType,
          systemPrompt: formData.systemPrompt || undefined,
          instructions: formData.instructions || undefined,
          capabilities: formData.capabilities.length > 0 ? formData.capabilities : undefined,
        };
        await onSubmit(updateData);
      } else {
        const createData: CreateAgentRequest = {
          agentKey: formData.agentKey,
          name: formData.name,
          description: formData.description || undefined,
          agentType: formData.agentType,
          systemPrompt: formData.systemPrompt || undefined,
          instructions: formData.instructions || undefined,
          capabilities: formData.capabilities.length > 0 ? formData.capabilities : undefined,
          organizationId: formData.organizationId || organizationId,
          projectId: formData.projectId || projectId,
        };
        await onSubmit(createData);
      }
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
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>{isEditing ? 'Edit Agent' : 'Create New Agent'}</DialogTitle>
      <DialogContent>
        {(error || validationError) && (
          <Alert severity="error" sx={{ mb: 2, mt: 1 }}>
            {error || validationError}
          </Alert>
        )}

        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          {/* Agent Key - only for new agents */}
          {!isEditing && (
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Agent Key"
                fullWidth
                required
                value={formData.agentKey}
                onChange={handleChange('agentKey')}
                error={!!validationError && !formData.agentKey.trim()}
                helperText="Unique identifier (lowercase, hyphens allowed)"
                placeholder="e.g., code-reviewer"
              />
            </Grid>
          )}

          {/* Name */}
          <Grid size={{ xs: 12, sm: isEditing ? 12 : 6 }}>
            <TextField
              label="Name"
              fullWidth
              required
              value={formData.name}
              onChange={handleChange('name')}
              error={!!validationError && !formData.name.trim()}
              placeholder="e.g., Code Reviewer Agent"
            />
          </Grid>

          {/* Agent Type */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Agent Type"
              fullWidth
              value={formData.agentType}
              onChange={handleChange('agentType')}
            >
              {AGENT_TYPES.map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          {/* Capabilities */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Autocomplete
              multiple
              freeSolo
              options={COMMON_CAPABILITIES}
              value={formData.capabilities}
              onChange={handleCapabilitiesChange}
              renderTags={(value, getTagProps) =>
                value.map((option, index) => (
                  <Chip
                    variant="outlined"
                    label={option}
                    size="small"
                    {...getTagProps({ index })}
                    key={option}
                  />
                ))
              }
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Capabilities"
                  placeholder="Add capability..."
                />
              )}
            />
          </Grid>

          {/* Description */}
          <Grid size={{ xs: 12 }}>
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={formData.description}
              onChange={handleChange('description')}
              placeholder="Brief description of what this agent does..."
            />
          </Grid>

          {/* System Prompt */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
              System Prompt
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={4}
              value={formData.systemPrompt}
              onChange={handleChange('systemPrompt')}
              placeholder="Base system prompt for the agent..."
            />
          </Grid>

          {/* Instructions */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
              Instructions (Markdown)
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={6}
              value={formData.instructions}
              onChange={handleChange('instructions')}
              placeholder="Detailed instructions for agent behavior..."
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isLoading || !formData.name.trim() || (!isEditing && !formData.agentKey.trim())}
        >
          {isLoading ? (isEditing ? 'Saving...' : 'Creating...') : (isEditing ? 'Save' : 'Create')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
