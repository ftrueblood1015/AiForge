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
  Publish as PublishIcon,
  Unpublished as UnpublishIcon,
} from '@mui/icons-material';
import { useSkillStore } from '../stores/skillStore';
import { SkillCard, SkillFilters, SkillForm } from '../components/skills';
import { CardSkeleton } from '../components/common';
import type { SkillListItem, SkillSearchParams, CreateSkillRequest, UpdateSkillRequest } from '../types';

export default function SkillList() {
  const {
    skills,
    currentSkill,
    isLoading,
    error,
    fetchSkills,
    fetchSkill,
    createSkill,
    updateSkill,
    deleteSkill,
    publishSkill,
    unpublishSkill,
    clearError,
    searchParams,
    setSearchParams,
  } = useSkillStore();

  const [filters, setFilters] = useState<SkillSearchParams>(searchParams);
  const [searchValue, setSearchValue] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [editingSkill, setEditingSkill] = useState<SkillListItem | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [skillToDelete, setSkillToDelete] = useState<SkillListItem | null>(null);
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [menuSkill, setMenuSkill] = useState<SkillListItem | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchSkills(filters);
  }, [fetchSkills, filters]);

  const handleFilterChange = (newFilters: SkillSearchParams) => {
    setFilters(newFilters);
    setSearchParams(newFilters);
  };

  const handleClearFilters = () => {
    const cleared: SkillSearchParams = {};
    setFilters(cleared);
    setSearchParams(cleared);
    setSearchValue('');
  };

  // Client-side filtering for search
  const filteredSkills = skills.filter((skill) => {
    if (searchValue) {
      const search = searchValue.toLowerCase();
      if (
        !skill.name.toLowerCase().includes(search) &&
        !skill.skillKey.toLowerCase().includes(search) &&
        !(skill.description?.toLowerCase().includes(search))
      ) {
        return false;
      }
    }
    return true;
  });

  const handleMenuOpen = (event: React.MouseEvent, skill: SkillListItem) => {
    setMenuAnchor(event.currentTarget as HTMLElement);
    setMenuSkill(skill);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setMenuSkill(null);
  };

  const handleCreateClick = () => {
    setEditingSkill(null);
    setFormError(null);
    setFormOpen(true);
  };

  const handleEditClick = async () => {
    if (menuSkill) {
      await fetchSkill(menuSkill.id);
      setEditingSkill(menuSkill);
      setFormError(null);
      setFormOpen(true);
    }
    handleMenuClose();
  };

  const handleDeleteClick = () => {
    if (menuSkill) {
      setSkillToDelete(menuSkill);
      setDeleteConfirmOpen(true);
    }
    handleMenuClose();
  };

  const handlePublishToggle = async () => {
    if (menuSkill) {
      try {
        if (menuSkill.isPublished) {
          await unpublishSkill(menuSkill.id);
        } else {
          await publishSkill(menuSkill.id);
        }
      } catch {
        // Error handled by store
      }
    }
    handleMenuClose();
  };

  const handleFormSubmit = async (data: CreateSkillRequest | UpdateSkillRequest) => {
    setIsSubmitting(true);
    setFormError(null);
    try {
      if (editingSkill) {
        await updateSkill(editingSkill.id, data as UpdateSkillRequest);
      } else {
        await createSkill(data as CreateSkillRequest);
      }
      setFormOpen(false);
      setEditingSkill(null);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'An error occurred');
      throw err;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleFormClose = () => {
    setFormOpen(false);
    setEditingSkill(null);
    setFormError(null);
  };

  const handleDeleteConfirm = async () => {
    if (skillToDelete) {
      try {
        await deleteSkill(skillToDelete.id);
        setDeleteConfirmOpen(false);
        setSkillToDelete(null);
      } catch {
        // Error handled by store
      }
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmOpen(false);
    setSkillToDelete(null);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Skills</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleCreateClick}
        >
          Create Skill
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      <SkillFilters
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
      ) : filteredSkills.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 6, bgcolor: 'background.paper', borderRadius: 1 }}>
          <Typography color="text.secondary" gutterBottom>
            {skills.length === 0 ? 'No skills configured' : 'No skills match your filters'}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {skills.length === 0
              ? 'Create your first skill to define reusable prompts and commands.'
              : 'Try adjusting your search or filter criteria.'}
          </Typography>
          {skills.length === 0 ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateClick}>
              Create Skill
            </Button>
          ) : (
            <Button onClick={handleClearFilters}>Clear Filters</Button>
          )}
        </Box>
      ) : (
        <Grid container spacing={2}>
          {filteredSkills.map((skill) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={skill.id}>
              <SkillCard
                skill={skill}
                onMenuClick={handleMenuOpen}
              />
            </Grid>
          ))}
        </Grid>
      )}

      {/* Results count */}
      {!isLoading && filteredSkills.length > 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Showing {filteredSkills.length} of {skills.length} skills
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
        <MenuItem onClick={handlePublishToggle}>
          <ListItemIcon>
            {menuSkill?.isPublished ? (
              <UnpublishIcon fontSize="small" />
            ) : (
              <PublishIcon fontSize="small" />
            )}
          </ListItemIcon>
          <ListItemText>
            {menuSkill?.isPublished ? 'Unpublish' : 'Publish'}
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
      <SkillForm
        open={formOpen}
        onClose={handleFormClose}
        onSubmit={handleFormSubmit}
        skill={editingSkill ? currentSkill || undefined : undefined}
        isLoading={isSubmitting}
        error={formError}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Delete Skill</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the skill "{skillToDelete?.name}"?
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
