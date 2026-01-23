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
  Typography,
} from '@mui/material';
import type { Skill, SkillCategory, CreateSkillRequest, UpdateSkillRequest } from '../../types';

interface SkillFormData {
  skillKey: string;
  name: string;
  description: string;
  category: SkillCategory;
  content: string;
  organizationId?: string;
  projectId?: string;
}

interface SkillFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSkillRequest | UpdateSkillRequest) => Promise<void>;
  skill?: Skill;
  isLoading?: boolean;
  error?: string | null;
  organizationId?: string;
  projectId?: string;
}

const SKILL_CATEGORIES: SkillCategory[] = [
  'Workflow',
  'Analysis',
  'Documentation',
  'Generation',
  'Testing',
  'Custom',
];

const initialFormData: SkillFormData = {
  skillKey: '',
  name: '',
  description: '',
  category: 'Custom',
  content: '',
};

export default function SkillForm({
  open,
  onClose,
  onSubmit,
  skill,
  isLoading = false,
  error = null,
  organizationId,
  projectId,
}: SkillFormProps) {
  const [formData, setFormData] = useState<SkillFormData>(initialFormData);
  const [validationError, setValidationError] = useState<string | null>(null);

  const isEditing = !!skill;

  useEffect(() => {
    if (skill) {
      setFormData({
        skillKey: skill.skillKey,
        name: skill.name,
        description: skill.description || '',
        category: skill.category,
        content: skill.content,
      });
    } else {
      setFormData({
        ...initialFormData,
        organizationId,
        projectId,
      });
    }
    setValidationError(null);
  }, [skill, open, organizationId, projectId]);

  const handleChange = (field: keyof SkillFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }));
    if (validationError) setValidationError(null);
  };

  const handleSubmit = async () => {
    if (!formData.name.trim()) {
      setValidationError('Name is required');
      return;
    }

    if (!isEditing && !formData.skillKey.trim()) {
      setValidationError('Skill Key is required');
      return;
    }

    if (!isEditing && !/^[a-z0-9-]+$/.test(formData.skillKey)) {
      setValidationError('Skill Key must be lowercase letters, numbers, and hyphens only');
      return;
    }

    if (!formData.content.trim()) {
      setValidationError('Content is required');
      return;
    }

    try {
      if (isEditing) {
        const updateData: UpdateSkillRequest = {
          name: formData.name,
          description: formData.description || undefined,
          category: formData.category,
          content: formData.content,
        };
        await onSubmit(updateData);
      } else {
        const createData: CreateSkillRequest = {
          skillKey: formData.skillKey,
          name: formData.name,
          description: formData.description || undefined,
          category: formData.category,
          content: formData.content,
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
      <DialogTitle>{isEditing ? 'Edit Skill' : 'Create New Skill'}</DialogTitle>
      <DialogContent>
        {(error || validationError) && (
          <Alert severity="error" sx={{ mb: 2, mt: 1 }}>
            {error || validationError}
          </Alert>
        )}

        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          {/* Skill Key - only for new skills */}
          {!isEditing && (
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                label="Skill Key"
                fullWidth
                required
                value={formData.skillKey}
                onChange={handleChange('skillKey')}
                error={!!validationError && !formData.skillKey.trim()}
                helperText="Used as slash command (e.g., /code-review)"
                placeholder="e.g., code-review"
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
              placeholder="e.g., Code Review Assistant"
            />
          </Grid>

          {/* Category */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Category"
              fullWidth
              value={formData.category}
              onChange={handleChange('category')}
            >
              {SKILL_CATEGORIES.map((category) => (
                <MenuItem key={category} value={category}>
                  {category}
                </MenuItem>
              ))}
            </TextField>
          </Grid>

          {/* Description */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Description"
              fullWidth
              value={formData.description}
              onChange={handleChange('description')}
              placeholder="Brief description..."
            />
          </Grid>

          {/* Content */}
          <Grid size={{ xs: 12 }}>
            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
              Skill Content (Markdown)
            </Typography>
            <TextField
              fullWidth
              required
              multiline
              rows={12}
              value={formData.content}
              onChange={handleChange('content')}
              error={!!validationError && !formData.content.trim()}
              placeholder="Enter the skill prompt/instructions in markdown format..."
              sx={{
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
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
          disabled={
            isLoading ||
            !formData.name.trim() ||
            !formData.content.trim() ||
            (!isEditing && !formData.skillKey.trim())
          }
        >
          {isLoading ? (isEditing ? 'Saving...' : 'Creating...') : isEditing ? 'Save' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
