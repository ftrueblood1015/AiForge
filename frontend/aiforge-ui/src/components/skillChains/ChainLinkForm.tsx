import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  Alert,
  MenuItem,
  Typography,
  Divider,
  Box,
} from '@mui/material';
import { skillsApi } from '../../api/skills';
import { agentsApi } from '../../api/agents';
import type {
  SkillChainLink,
  SkillListItem,
  AgentListItem,
  TransitionType,
  CreateSkillChainLinkRequest,
  UpdateSkillChainLinkRequest,
} from '../../types';

interface ChainLinkFormData {
  name: string;
  description: string;
  skillId: string;
  agentId: string;
  maxRetries: number;
  onSuccessTransition: TransitionType;
  onSuccessTargetLinkId: string;
  onFailureTransition: TransitionType;
  onFailureTargetLinkId: string;
}

interface ChainLinkFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSkillChainLinkRequest | UpdateSkillChainLinkRequest) => Promise<void>;
  link?: SkillChainLink | null;
  chainLinks?: SkillChainLink[];
  isLoading?: boolean;
  error?: string | null;
}

const SUCCESS_TRANSITIONS: { value: TransitionType; label: string }[] = [
  { value: 'NextLink', label: 'Next Link' },
  { value: 'GoToLink', label: 'Go To Link' },
  { value: 'Complete', label: 'Complete Chain' },
];

const FAILURE_TRANSITIONS: { value: TransitionType; label: string }[] = [
  { value: 'Retry', label: 'Retry' },
  { value: 'GoToLink', label: 'Go To Link' },
  { value: 'Escalate', label: 'Escalate (Human Intervention)' },
];

const initialFormData: ChainLinkFormData = {
  name: '',
  description: '',
  skillId: '',
  agentId: '',
  maxRetries: 3,
  onSuccessTransition: 'NextLink',
  onSuccessTargetLinkId: '',
  onFailureTransition: 'Retry',
  onFailureTargetLinkId: '',
};

export default function ChainLinkForm({
  open,
  onClose,
  onSubmit,
  link,
  chainLinks = [],
  isLoading = false,
  error = null,
}: ChainLinkFormProps) {
  const [formData, setFormData] = useState<ChainLinkFormData>(initialFormData);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [skills, setSkills] = useState<SkillListItem[]>([]);
  const [agents, setAgents] = useState<AgentListItem[]>([]);
  const [loadingOptions, setLoadingOptions] = useState(false);

  const isEditing = !!link;

  // Fetch skills and agents when dialog opens
  useEffect(() => {
    if (open) {
      const fetchOptions = async () => {
        setLoadingOptions(true);
        try {
          const [skillsResponse, agentsResponse] = await Promise.all([
            skillsApi.getAll({ publishedOnly: true }),
            agentsApi.getAll({}),
          ]);
          setSkills(skillsResponse.items || []);
          setAgents(agentsResponse.items || []);
        } catch (err) {
          console.error('Failed to fetch options:', err);
        } finally {
          setLoadingOptions(false);
        }
      };
      fetchOptions();
    }
  }, [open]);

  // Initialize form when link changes
  useEffect(() => {
    if (link) {
      setFormData({
        name: link.name,
        description: link.description || '',
        skillId: link.skillId,
        agentId: link.agentId || '',
        maxRetries: link.maxRetries,
        onSuccessTransition: link.onSuccessTransition,
        onSuccessTargetLinkId: link.onSuccessTargetLinkId || '',
        onFailureTransition: link.onFailureTransition,
        onFailureTargetLinkId: link.onFailureTargetLinkId || '',
      });
    } else {
      setFormData(initialFormData);
    }
    setValidationError(null);
  }, [link, open]);

  const handleChange = (field: keyof ChainLinkFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = field === 'maxRetries'
      ? parseInt(e.target.value, 10) || 3
      : e.target.value;
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (validationError) setValidationError(null);
  };

  const handleSubmit = async () => {
    if (!formData.name.trim()) {
      setValidationError('Name is required');
      return;
    }

    if (!formData.skillId) {
      setValidationError('Skill is required');
      return;
    }

    if (formData.onSuccessTransition === 'GoToLink' && !formData.onSuccessTargetLinkId) {
      setValidationError('Target link is required when using "Go To Link" for success transition');
      return;
    }

    if (formData.onFailureTransition === 'GoToLink' && !formData.onFailureTargetLinkId) {
      setValidationError('Target link is required when using "Go To Link" for failure transition');
      return;
    }

    if (formData.maxRetries < 1 || formData.maxRetries > 10) {
      setValidationError('Max retries must be between 1 and 10');
      return;
    }

    try {
      const data: CreateSkillChainLinkRequest | UpdateSkillChainLinkRequest = {
        name: formData.name,
        description: formData.description || undefined,
        skillId: formData.skillId,
        agentId: formData.agentId || undefined,
        maxRetries: formData.maxRetries,
        onSuccessTransition: formData.onSuccessTransition,
        onSuccessTargetLinkId: formData.onSuccessTransition === 'GoToLink' ? formData.onSuccessTargetLinkId : undefined,
        onFailureTransition: formData.onFailureTransition,
        onFailureTargetLinkId: formData.onFailureTransition === 'GoToLink' ? formData.onFailureTargetLinkId : undefined,
      };
      await onSubmit(data);
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

  // Filter out current link from target options
  const availableTargetLinks = chainLinks.filter((l) => l.id !== link?.id);

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>{isEditing ? 'Edit Link' : 'Add New Link'}</DialogTitle>
      <DialogContent>
        {(error || validationError) && (
          <Alert severity="error" sx={{ mb: 2, mt: 1 }}>
            {error || validationError}
          </Alert>
        )}

        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          {/* Basic Info */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Basic Information
            </Typography>
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Name"
              fullWidth
              required
              value={formData.name}
              onChange={handleChange('name')}
              error={!!validationError && !formData.name.trim()}
              placeholder="e.g., Research, Plan, Implement"
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Max Retries"
              type="number"
              fullWidth
              value={formData.maxRetries}
              onChange={handleChange('maxRetries')}
              inputProps={{ min: 1, max: 10 }}
              helperText="1-10 retries before escalation"
            />
          </Grid>

          <Grid size={{ xs: 12 }}>
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={formData.description}
              onChange={handleChange('description')}
              placeholder="What does this link do?"
            />
          </Grid>

          <Grid size={{ xs: 12 }}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          {/* Skill & Agent Selection */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Execution
            </Typography>
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Skill"
              fullWidth
              required
              value={formData.skillId}
              onChange={handleChange('skillId')}
              error={!!validationError && !formData.skillId}
              disabled={loadingOptions}
              helperText={loadingOptions ? 'Loading skills...' : 'Skill to execute at this step'}
            >
              {skills.map((skill) => (
                <MenuItem key={skill.id} value={skill.id}>
                  <Box>
                    <Typography variant="body2">{skill.name}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {skill.skillKey}
                    </Typography>
                  </Box>
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Agent (Optional)"
              fullWidth
              value={formData.agentId}
              onChange={handleChange('agentId')}
              disabled={loadingOptions}
              helperText="Specialized agent to use"
            >
              <MenuItem value="">
                <em>None (use default)</em>
              </MenuItem>
              {agents.map((agent) => (
                <MenuItem key={agent.id} value={agent.id}>
                  <Box>
                    <Typography variant="body2">{agent.name}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {agent.agentKey}
                    </Typography>
                  </Box>
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          <Grid size={{ xs: 12 }}>
            <Divider sx={{ my: 1 }} />
          </Grid>

          {/* Transitions */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Transitions
            </Typography>
          </Grid>

          {/* Success Transition */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="On Success"
              fullWidth
              value={formData.onSuccessTransition}
              onChange={handleChange('onSuccessTransition')}
            >
              {SUCCESS_TRANSITIONS.map((t) => (
                <MenuItem key={t.value} value={t.value}>
                  {t.label}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          {formData.onSuccessTransition === 'GoToLink' && (
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                select
                label="Success Target Link"
                fullWidth
                required
                value={formData.onSuccessTargetLinkId}
                onChange={handleChange('onSuccessTargetLinkId')}
                error={!!validationError && !formData.onSuccessTargetLinkId}
              >
                {availableTargetLinks.map((l) => (
                  <MenuItem key={l.id} value={l.id}>
                    {l.position + 1}. {l.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          )}

          {/* Failure Transition */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="On Failure"
              fullWidth
              value={formData.onFailureTransition}
              onChange={handleChange('onFailureTransition')}
            >
              {FAILURE_TRANSITIONS.map((t) => (
                <MenuItem key={t.value} value={t.value}>
                  {t.label}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          {formData.onFailureTransition === 'GoToLink' && (
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                select
                label="Failure Target Link"
                fullWidth
                required
                value={formData.onFailureTargetLinkId}
                onChange={handleChange('onFailureTargetLinkId')}
                error={!!validationError && !formData.onFailureTargetLinkId}
              >
                {availableTargetLinks.map((l) => (
                  <MenuItem key={l.id} value={l.id}>
                    {l.position + 1}. {l.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          )}
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isLoading || !formData.name.trim() || !formData.skillId}
        >
          {isLoading ? (isEditing ? 'Saving...' : 'Adding...') : isEditing ? 'Save' : 'Add Link'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
