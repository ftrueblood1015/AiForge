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
  Typography,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { SkillChain, CreateSkillChainRequest, UpdateSkillChainRequest } from '../../types';

interface ChainFormData {
  chainKey: string;
  name: string;
  description: string;
  inputSchema: string;
  maxTotalFailures: number;
  organizationId?: string;
  projectId?: string;
}

interface ChainFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSkillChainRequest | UpdateSkillChainRequest) => Promise<void>;
  chain?: SkillChain;
  isLoading?: boolean;
  error?: string | null;
  organizationId?: string;
  projectId?: string;
}

const initialFormData: ChainFormData = {
  chainKey: '',
  name: '',
  description: '',
  inputSchema: '',
  maxTotalFailures: 5,
};

export default function ChainForm({
  open,
  onClose,
  onSubmit,
  chain,
  isLoading = false,
  error = null,
  organizationId,
  projectId,
}: ChainFormProps) {
  const [formData, setFormData] = useState<ChainFormData>(initialFormData);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [schemaExpanded, setSchemaExpanded] = useState(false);

  const isEditing = !!chain;

  useEffect(() => {
    if (chain) {
      setFormData({
        chainKey: chain.chainKey,
        name: chain.name,
        description: chain.description || '',
        inputSchema: chain.inputSchema || '',
        maxTotalFailures: chain.maxTotalFailures,
      });
      setSchemaExpanded(!!chain.inputSchema);
    } else {
      setFormData({
        ...initialFormData,
        organizationId,
        projectId,
      });
      setSchemaExpanded(false);
    }
    setValidationError(null);
  }, [chain, open, organizationId, projectId]);

  const handleChange = (field: keyof ChainFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = field === 'maxTotalFailures'
      ? parseInt(e.target.value, 10) || 5
      : e.target.value;
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (validationError) setValidationError(null);
  };

  const validateInputSchema = (schema: string): boolean => {
    if (!schema.trim()) return true;
    try {
      JSON.parse(schema);
      return true;
    } catch {
      return false;
    }
  };

  const handleSubmit = async () => {
    if (!formData.name.trim()) {
      setValidationError('Name is required');
      return;
    }

    if (!isEditing && !formData.chainKey.trim()) {
      setValidationError('Chain Key is required');
      return;
    }

    if (!isEditing && !/^[a-z0-9-]+$/.test(formData.chainKey)) {
      setValidationError('Chain Key must be lowercase letters, numbers, and hyphens only');
      return;
    }

    if (formData.inputSchema && !validateInputSchema(formData.inputSchema)) {
      setValidationError('Input Schema must be valid JSON');
      return;
    }

    if (formData.maxTotalFailures < 1 || formData.maxTotalFailures > 100) {
      setValidationError('Max Total Failures must be between 1 and 100');
      return;
    }

    try {
      if (isEditing) {
        const updateData: UpdateSkillChainRequest = {
          name: formData.name,
          description: formData.description || undefined,
          inputSchema: formData.inputSchema || undefined,
          maxTotalFailures: formData.maxTotalFailures,
        };
        await onSubmit(updateData);
      } else {
        const createData: CreateSkillChainRequest = {
          chainKey: formData.chainKey,
          name: formData.name,
          description: formData.description || undefined,
          inputSchema: formData.inputSchema || undefined,
          maxTotalFailures: formData.maxTotalFailures,
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
      <DialogTitle>{isEditing ? 'Edit Skill Chain' : 'Create New Skill Chain'}</DialogTitle>
      <DialogContent>
        {(error || validationError) && (
          <Alert severity="error" sx={{ mb: 2, mt: 1 }}>
            {error || validationError}
          </Alert>
        )}

        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          {/* Chain Key - only for new chains */}
          {!isEditing && (
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Chain Key"
                fullWidth
                required
                value={formData.chainKey}
                onChange={handleChange('chainKey')}
                error={!!validationError && !formData.chainKey.trim()}
                helperText="Unique identifier (e.g., feature-implementation)"
                placeholder="e.g., feature-implementation"
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
              placeholder="e.g., Feature Implementation Workflow"
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
              placeholder="Describe what this skill chain does..."
            />
          </Grid>

          {/* Max Total Failures */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Max Total Failures"
              type="number"
              fullWidth
              value={formData.maxTotalFailures}
              onChange={handleChange('maxTotalFailures')}
              helperText="Maximum failures before human intervention (1-100)"
              inputProps={{ min: 1, max: 100 }}
            />
          </Grid>

          {/* Input Schema (Advanced) */}
          <Grid size={{ xs: 12 }}>
            <Accordion
              expanded={schemaExpanded}
              onChange={(_, expanded) => setSchemaExpanded(expanded)}
              sx={{ boxShadow: 'none', border: 1, borderColor: 'divider' }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2" color="text.secondary">
                  Input Schema (Optional - JSON)
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <TextField
                  fullWidth
                  multiline
                  rows={6}
                  value={formData.inputSchema}
                  onChange={handleChange('inputSchema')}
                  error={!!validationError && formData.inputSchema !== '' && !validateInputSchema(formData.inputSchema)}
                  placeholder='{"type": "object", "properties": {...}, "required": [...]}'
                  sx={{
                    '& .MuiInputBase-input': {
                      fontFamily: 'monospace',
                      fontSize: '0.875rem',
                    },
                  }}
                />
                <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                  Define required inputs for this chain using JSON Schema format
                </Typography>
              </AccordionDetails>
            </Accordion>
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
          disabled={
            isLoading ||
            !formData.name.trim() ||
            (!isEditing && !formData.chainKey.trim())
          }
        >
          {isLoading ? (isEditing ? 'Saving...' : 'Creating...') : isEditing ? 'Save' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
