import React from 'react';
import {
  Box,
  ToggleButtonGroup,
  ToggleButton,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  FormHelperText,
} from '@mui/material';
import {
  Business as OrgIcon,
  Folder as ProjectIcon,
} from '@mui/icons-material';
import type { ConfigurationScope } from '../../types';

interface ScopeSelectorProps {
  scope: ConfigurationScope;
  organizationId: string;
  projectId?: string;
  onScopeChange: (scope: ConfigurationScope, orgId: string, projectId?: string) => void;
  disabled?: boolean;
  organizations?: Array<{ id: string; name: string }>;
  projects?: Array<{ id: string; name: string; organizationId: string }>;
  helperText?: string;
  error?: boolean;
}

export default function ScopeSelector({
  scope,
  organizationId,
  projectId,
  onScopeChange,
  disabled = false,
  organizations = [],
  projects = [],
  helperText,
  error = false,
}: ScopeSelectorProps) {
  const handleScopeTypeChange = (
    _event: React.MouseEvent<HTMLElement>,
    newScope: ConfigurationScope | null
  ) => {
    if (newScope) {
      if (newScope === 'Organization') {
        onScopeChange(newScope, organizationId, undefined);
      } else {
        onScopeChange(newScope, organizationId, projectId);
      }
    }
  };

  const handleOrganizationChange = (event: { target: { value: string } }) => {
    const newOrgId = event.target.value;
    onScopeChange(scope, newOrgId, scope === 'Project' ? undefined : undefined);
  };

  const handleProjectChange = (event: { target: { value: string } }) => {
    const newProjectId = event.target.value;
    // Find the project to get its organization
    const project = projects.find((p) => p.id === newProjectId);
    if (project) {
      onScopeChange('Project', project.organizationId, newProjectId);
    }
  };

  // Filter projects by selected organization
  const filteredProjects = projects.filter((p) => p.organizationId === organizationId);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Scope Type Toggle */}
      <Box>
        <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
          Scope Level
        </Typography>
        <ToggleButtonGroup
          value={scope}
          exclusive
          onChange={handleScopeTypeChange}
          disabled={disabled}
          size="small"
          fullWidth
        >
          <ToggleButton value="Organization" sx={{ gap: 1 }}>
            <OrgIcon fontSize="small" />
            Organization
          </ToggleButton>
          <ToggleButton value="Project" sx={{ gap: 1 }}>
            <ProjectIcon fontSize="small" />
            Project
          </ToggleButton>
        </ToggleButtonGroup>
        {helperText && (
          <FormHelperText error={error} sx={{ ml: 0 }}>
            {helperText}
          </FormHelperText>
        )}
      </Box>

      {/* Organization Selector */}
      <FormControl fullWidth size="small" disabled={disabled} error={error && !organizationId}>
        <InputLabel>Organization</InputLabel>
        <Select
          value={organizationId}
          label="Organization"
          onChange={handleOrganizationChange}
        >
          {organizations.map((org) => (
            <MenuItem key={org.id} value={org.id}>
              {org.name}
            </MenuItem>
          ))}
        </Select>
        {scope === 'Organization' && (
          <FormHelperText>
            Configuration will be available to all projects in this organization
          </FormHelperText>
        )}
      </FormControl>

      {/* Project Selector (only shown when scope is Project) */}
      {scope === 'Project' && (
        <FormControl
          fullWidth
          size="small"
          disabled={disabled || !organizationId}
          error={error && !projectId}
        >
          <InputLabel>Project</InputLabel>
          <Select
            value={projectId || ''}
            label="Project"
            onChange={handleProjectChange}
          >
            {filteredProjects.length === 0 ? (
              <MenuItem disabled value="">
                No projects in selected organization
              </MenuItem>
            ) : (
              filteredProjects.map((project) => (
                <MenuItem key={project.id} value={project.id}>
                  {project.name}
                </MenuItem>
              ))
            )}
          </Select>
          <FormHelperText>
            Configuration will only be available to this project
          </FormHelperText>
        </FormControl>
      )}

      {/* Scope Summary */}
      <Box
        sx={{
          p: 1.5,
          bgcolor: 'action.hover',
          borderRadius: 1,
          display: 'flex',
          alignItems: 'center',
          gap: 1,
        }}
      >
        {scope === 'Organization' ? (
          <OrgIcon fontSize="small" color="primary" />
        ) : (
          <ProjectIcon fontSize="small" color="secondary" />
        )}
        <Typography variant="body2" color="text.secondary">
          {scope === 'Organization'
            ? 'Available organization-wide'
            : 'Available to specific project only'}
        </Typography>
      </Box>
    </Box>
  );
}
