import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Alert,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Skeleton,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material';
import {
  ArrowBack as BackIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Publish as PublishIcon,
  Unpublished as UnpublishIcon,
  Add as AddIcon,
  KeyboardArrowUp as MoveUpIcon,
  KeyboardArrowDown as MoveDownIcon,
  ExpandMore as ExpandMoreIcon,
  Link as LinkIcon,
} from '@mui/icons-material';
import { useSkillChainStore } from '../stores/skillChainStore';
import { ChainForm, ChainLinkItem } from '../components/skillChains';
import ChainLinkForm from '../components/skillChains/ChainLinkForm';
import type { SkillChainLink, CreateSkillChainLinkRequest, UpdateSkillChainLinkRequest } from '../types';

export default function SkillChainDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const {
    currentChain: chain,
    isLoading,
    error,
    fetchChain,
    updateChain,
    deleteChain,
    publishChain,
    unpublishChain,
    addLink,
    updateLink,
    removeLink,
    reorderLinks,
    clearCurrentChain,
    clearError,
  } = useSkillChainStore();

  // Dialog states
  const [editFormOpen, setEditFormOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [linkFormOpen, setLinkFormOpen] = useState(false);
  const [editingLink, setEditingLink] = useState<SkillChainLink | null>(null);
  const [deleteLinkConfirmOpen, setDeleteLinkConfirmOpen] = useState(false);
  const [linkToDelete, setLinkToDelete] = useState<SkillChainLink | null>(null);

  useEffect(() => {
    if (id) {
      fetchChain(id);
    }
    return () => {
      clearCurrentChain();
    };
  }, [id, fetchChain, clearCurrentChain]);

  const handleBack = () => {
    navigate('/skill-chains');
  };

  const handleEditChain = () => {
    setEditFormOpen(true);
  };

  const handleEditFormClose = () => {
    setEditFormOpen(false);
  };

  const handleEditFormSubmit = async (data: any) => {
    if (chain) {
      await updateChain(chain.id, data);
      setEditFormOpen(false);
    }
  };

  const handlePublishToggle = async () => {
    if (chain) {
      if (chain.isPublished) {
        await unpublishChain(chain.id);
      } else {
        await publishChain(chain.id);
      }
    }
  };

  const handleDeleteClick = () => {
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (chain) {
      await deleteChain(chain.id);
      navigate('/skill-chains');
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmOpen(false);
  };

  // Link handlers
  const handleAddLink = () => {
    setEditingLink(null);
    setLinkFormOpen(true);
  };

  const handleEditLink = (link: SkillChainLink) => {
    setEditingLink(link);
    setLinkFormOpen(true);
  };

  const handleLinkFormClose = () => {
    setLinkFormOpen(false);
    setEditingLink(null);
  };

  const handleLinkFormSubmit = async (data: CreateSkillChainLinkRequest | UpdateSkillChainLinkRequest) => {
    if (chain) {
      if (editingLink) {
        await updateLink(editingLink.id, data as UpdateSkillChainLinkRequest);
      } else {
        await addLink(chain.id, data as CreateSkillChainLinkRequest);
      }
      setLinkFormOpen(false);
      setEditingLink(null);
    }
  };

  const handleDeleteLink = (link: SkillChainLink) => {
    setLinkToDelete(link);
    setDeleteLinkConfirmOpen(true);
  };

  const handleDeleteLinkConfirm = async () => {
    if (chain && linkToDelete) {
      await removeLink(chain.id, linkToDelete.id);
      setDeleteLinkConfirmOpen(false);
      setLinkToDelete(null);
    }
  };

  const handleDeleteLinkCancel = () => {
    setDeleteLinkConfirmOpen(false);
    setLinkToDelete(null);
  };

  const handleMoveLink = async (link: SkillChainLink, direction: 'up' | 'down') => {
    if (!chain) return;

    const links = [...chain.links].sort((a, b) => a.position - b.position);
    const currentIndex = links.findIndex((l) => l.id === link.id);
    const newIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1;

    if (newIndex < 0 || newIndex >= links.length) return;

    // Swap positions
    const newOrder = links.map((l) => l.id);
    [newOrder[currentIndex], newOrder[newIndex]] = [newOrder[newIndex], newOrder[currentIndex]];

    await reorderLinks(chain.id, newOrder);
  };

  if (isLoading && !chain) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <Skeleton variant="circular" width={40} height={40} />
          <Skeleton variant="text" width={300} height={40} />
        </Box>
        <Skeleton variant="rounded" height={200} sx={{ mb: 2 }} />
        <Skeleton variant="rounded" height={400} />
      </Box>
    );
  }

  if (!chain && !isLoading) {
    return (
      <Box>
        <Button startIcon={<BackIcon />} onClick={handleBack} sx={{ mb: 2 }}>
          Back to Skill Chains
        </Button>
        <Alert severity="error">
          Skill chain not found
        </Alert>
      </Box>
    );
  }

  if (!chain) return null;

  const sortedLinks = [...chain.links].sort((a, b) => a.position - b.position);

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={handleBack}>
            <BackIcon />
          </IconButton>
          <Box>
            <Typography variant="h5" fontWeight={600}>
              {chain.name}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
              {chain.chainKey}
            </Typography>
          </Box>
          <Chip
            label={chain.isPublished ? 'Published' : 'Draft'}
            color={chain.isPublished ? 'success' : 'default'}
            size="small"
          />
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Tooltip title={chain.isPublished ? 'Unpublish' : 'Publish'}>
            <IconButton onClick={handlePublishToggle}>
              {chain.isPublished ? <UnpublishIcon /> : <PublishIcon />}
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit chain">
            <IconButton onClick={handleEditChain}>
              <EditIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete chain">
            <IconButton color="error" onClick={handleDeleteClick}>
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={clearError}>
          {error}
        </Alert>
      )}

      {/* Chain Info */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Description
        </Typography>
        <Typography variant="body1" sx={{ mb: 2 }}>
          {chain.description || 'No description provided'}
        </Typography>

        <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Scope
            </Typography>
            <Typography variant="body2">{chain.scope}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Max Total Failures
            </Typography>
            <Typography variant="body2">{chain.maxTotalFailures}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Links
            </Typography>
            <Typography variant="body2">{chain.links.length}</Typography>
          </Box>
        </Box>

        {chain.inputSchema && (
          <Accordion sx={{ mt: 2, boxShadow: 'none', border: 1, borderColor: 'divider' }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="subtitle2" color="text.secondary">
                Input Schema
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box
                component="pre"
                sx={{
                  p: 2,
                  bgcolor: 'grey.100',
                  borderRadius: 1,
                  overflow: 'auto',
                  fontSize: '0.875rem',
                  fontFamily: 'monospace',
                  m: 0,
                }}
              >
                {JSON.stringify(JSON.parse(chain.inputSchema), null, 2)}
              </Box>
            </AccordionDetails>
          </Accordion>
        )}
      </Paper>

      {/* Links Section */}
      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <LinkIcon color="primary" />
            <Typography variant="h6">Chain Links</Typography>
            <Chip label={sortedLinks.length} size="small" />
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleAddLink}
            size="small"
          >
            Add Link
          </Button>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {sortedLinks.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography color="text.secondary" gutterBottom>
              No links configured
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Add links to define the workflow steps in this chain.
            </Typography>
            <Button variant="outlined" startIcon={<AddIcon />} onClick={handleAddLink}>
              Add First Link
            </Button>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {sortedLinks.map((link, index) => (
              <Box key={link.id} sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
                {/* Reorder buttons */}
                <Box sx={{ display: 'flex', flexDirection: 'column', pt: 1 }}>
                  <Tooltip title="Move up">
                    <span>
                      <IconButton
                        size="small"
                        onClick={() => handleMoveLink(link, 'up')}
                        disabled={index === 0}
                      >
                        <MoveUpIcon fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                  <Tooltip title="Move down">
                    <span>
                      <IconButton
                        size="small"
                        onClick={() => handleMoveLink(link, 'down')}
                        disabled={index === sortedLinks.length - 1}
                      >
                        <MoveDownIcon fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                </Box>
                {/* Link item */}
                <Box sx={{ flex: 1 }}>
                  <ChainLinkItem
                    link={link}
                    onEdit={handleEditLink}
                    onDelete={handleDeleteLink}
                  />
                </Box>
              </Box>
            ))}
          </Box>
        )}
      </Paper>

      {/* Edit Chain Dialog */}
      <ChainForm
        open={editFormOpen}
        onClose={handleEditFormClose}
        onSubmit={handleEditFormSubmit}
        chain={chain}
        isLoading={isLoading}
        error={error}
      />

      {/* Delete Chain Confirmation */}
      <Dialog open={deleteConfirmOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Delete Skill Chain</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the skill chain "{chain.name}"?
            This action cannot be undone. All link configurations will be lost.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDeleteCancel}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      {/* Link Form Dialog */}
      <ChainLinkForm
        open={linkFormOpen}
        onClose={handleLinkFormClose}
        onSubmit={handleLinkFormSubmit}
        link={editingLink}
        chainLinks={sortedLinks}
        isLoading={isLoading}
        error={error}
      />

      {/* Delete Link Confirmation */}
      <Dialog open={deleteLinkConfirmOpen} onClose={handleDeleteLinkCancel}>
        <DialogTitle>Delete Link</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the link "{linkToDelete?.name}"?
            This will affect the chain workflow.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDeleteLinkCancel}>Cancel</Button>
          <Button onClick={handleDeleteLinkConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
