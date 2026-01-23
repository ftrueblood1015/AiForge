import { useState, useEffect, useCallback } from 'react';
import {
  Card,
  CardContent,
  CardHeader,
  Box,
  Typography,
  IconButton,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Alert,
  Button,
  TextField,
  CircularProgress,
  Tooltip,
} from '@mui/material';
import {
  Edit as EditIcon,
  ExpandMore as ExpandMoreIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  LightbulbOutlined as FocusIcon,
  Gavel as DecisionIcon,
  CheckCircleOutline as ResolvedIcon,
  ArrowForward as NextStepIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import type { ContextHelper, UpdateContextRequest } from '../../types';

interface ContextHelperPanelProps {
  context: ContextHelper;
  onUpdate: (request: UpdateContextRequest) => Promise<void>;
  isLoading?: boolean;
  isStale?: boolean;
  staleWarning?: string | null;
}

// Estimate byte size of context
function estimateByteSize(context: Partial<ContextHelper>): number {
  const json = JSON.stringify(context);
  return new TextEncoder().encode(json).length;
}

const MAX_SIZE_BYTES = 2048;
const WARNING_SIZE_BYTES = 1536; // 1.5KB

export default function ContextHelperPanel({
  context,
  onUpdate,
  isLoading = false,
  isStale = false,
  staleWarning,
}: ContextHelperPanelProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editedFocus, setEditedFocus] = useState(context.currentFocus);
  const [editedNextSteps, setEditedNextSteps] = useState(context.nextSteps.join('\n'));
  const [newDecision, setNewDecision] = useState('');
  const [newBlocker, setNewBlocker] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const handleStartEdit = () => {
    setEditedFocus(context.currentFocus);
    setEditedNextSteps(context.nextSteps.join('\n'));
    setNewDecision('');
    setNewBlocker('');
    setIsEditing(true);
  };

  const handleCancel = useCallback(() => {
    setIsEditing(false);
  }, []);

  const handleSave = useCallback(async () => {
    setIsSaving(true);
    try {
      const request: UpdateContextRequest = {};

      if (editedFocus !== context.currentFocus) {
        request.currentFocus = editedFocus;
      }

      const newSteps = editedNextSteps.split('\n').filter((s) => s.trim());
      if (JSON.stringify(newSteps) !== JSON.stringify(context.nextSteps)) {
        request.replaceNextSteps = newSteps;
      }

      if (newDecision.trim()) {
        request.appendKeyDecisions = [newDecision.trim()];
      }

      if (newBlocker.trim()) {
        request.appendBlockersResolved = [newBlocker.trim()];
      }

      if (Object.keys(request).length > 0) {
        await onUpdate(request);
      }

      setIsEditing(false);
    } finally {
      setIsSaving(false);
    }
  }, [editedFocus, editedNextSteps, newDecision, newBlocker, context, onUpdate]);

  // Keyboard shortcuts: Ctrl+S to save, Escape to cancel
  useEffect(() => {
    if (!isEditing) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        handleCancel();
      } else if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        if (!isOverLimit && !isSaving) {
          handleSave();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isEditing, isOverLimit, isSaving, handleCancel, handleSave]);

  // Calculate estimated size for warning
  const estimatedContext = {
    currentFocus: editedFocus,
    keyDecisions: newDecision ? [...context.keyDecisions, newDecision] : context.keyDecisions,
    blockersResolved: newBlocker ? [...context.blockersResolved, newBlocker] : context.blockersResolved,
    nextSteps: editedNextSteps.split('\n').filter((s) => s.trim()),
  };
  const currentSize = estimateByteSize(estimatedContext);
  const sizePercent = (currentSize / MAX_SIZE_BYTES) * 100;
  const isOverLimit = currentSize > MAX_SIZE_BYTES;
  const isNearLimit = currentSize > WARNING_SIZE_BYTES;

  return (
    <Card>
      <CardHeader
        title="Context Helper"
        action={
          !isEditing ? (
            <IconButton onClick={handleStartEdit} disabled={isLoading}>
              <EditIcon />
            </IconButton>
          ) : (
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Tooltip title="Cancel (Esc)">
                <IconButton onClick={handleCancel} disabled={isSaving}>
                  <CloseIcon />
                </IconButton>
              </Tooltip>
              <Tooltip title="Save (Ctrl+S)">
                <span>
                  <IconButton
                    onClick={handleSave}
                    disabled={isSaving || isOverLimit}
                    color="primary"
                  >
                    {isSaving ? <CircularProgress size={24} /> : <CheckIcon />}
                  </IconButton>
                </span>
              </Tooltip>
            </Box>
          )
        }
      />
      <CardContent>
        {/* Staleness Warning */}
        {isStale && staleWarning && (
          <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
            {staleWarning}
          </Alert>
        )}

        {/* Size Warning */}
        {isEditing && isNearLimit && (
          <Alert severity={isOverLimit ? 'error' : 'warning'} sx={{ mb: 2 }}>
            Context size: {currentSize} / {MAX_SIZE_BYTES} bytes ({sizePercent.toFixed(0)}%)
            {isOverLimit && ' - Reduce content to save'}
          </Alert>
        )}

        {/* Current Focus */}
        <Box
          sx={{
            p: 2,
            backgroundColor: 'primary.main',
            color: 'primary.contrastText',
            borderRadius: 1,
            mb: 2,
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <FocusIcon />
            <Typography variant="subtitle2">Current Focus</Typography>
          </Box>
          {isEditing ? (
            <TextField
              fullWidth
              multiline
              rows={2}
              value={editedFocus}
              onChange={(e) => setEditedFocus(e.target.value)}
              placeholder="What are you currently working on?"
              sx={{
                '& .MuiOutlinedInput-root': {
                  backgroundColor: 'rgba(255,255,255,0.1)',
                  color: 'inherit',
                },
              }}
            />
          ) : (
            <Typography variant="body1">
              {context.currentFocus || 'No focus set'}
            </Typography>
          )}
        </Box>

        {/* Key Decisions */}
        <Accordion defaultExpanded={context.keyDecisions.length > 0}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <DecisionIcon color="info" />
              <Typography>Key Decisions</Typography>
              <Chip label={context.keyDecisions.length} size="small" />
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            {context.keyDecisions.length === 0 && !isEditing ? (
              <Typography color="text.secondary">No decisions recorded yet.</Typography>
            ) : (
              <List dense disablePadding>
                {context.keyDecisions.map((decision, i) => (
                  <ListItem key={i} disableGutters>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <DecisionIcon fontSize="small" color="info" />
                    </ListItemIcon>
                    <ListItemText primary={decision} />
                  </ListItem>
                ))}
              </List>
            )}
            {isEditing && (
              <TextField
                fullWidth
                size="small"
                placeholder="Add a key decision..."
                value={newDecision}
                onChange={(e) => setNewDecision(e.target.value)}
                sx={{ mt: 1 }}
              />
            )}
          </AccordionDetails>
        </Accordion>

        {/* Blockers Resolved */}
        <Accordion defaultExpanded={context.blockersResolved.length > 0}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <ResolvedIcon color="success" />
              <Typography>Blockers Resolved</Typography>
              <Chip label={context.blockersResolved.length} size="small" />
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            {context.blockersResolved.length === 0 && !isEditing ? (
              <Typography color="text.secondary">No blockers resolved yet.</Typography>
            ) : (
              <List dense disablePadding>
                {context.blockersResolved.map((blocker, i) => (
                  <ListItem key={i} disableGutters>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <ResolvedIcon fontSize="small" color="success" />
                    </ListItemIcon>
                    <ListItemText primary={blocker} />
                  </ListItem>
                ))}
              </List>
            )}
            {isEditing && (
              <TextField
                fullWidth
                size="small"
                placeholder="Add a resolved blocker..."
                value={newBlocker}
                onChange={(e) => setNewBlocker(e.target.value)}
                sx={{ mt: 1 }}
              />
            )}
          </AccordionDetails>
        </Accordion>

        {/* Next Steps */}
        <Accordion defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <NextStepIcon color="warning" />
              <Typography>Next Steps</Typography>
              <Chip label={context.nextSteps.length} size="small" />
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            {isEditing ? (
              <TextField
                fullWidth
                multiline
                rows={4}
                value={editedNextSteps}
                onChange={(e) => setEditedNextSteps(e.target.value)}
                placeholder="Enter each step on a new line..."
                helperText="One step per line"
              />
            ) : context.nextSteps.length === 0 ? (
              <Typography color="text.secondary">No next steps defined.</Typography>
            ) : (
              <List dense disablePadding>
                {context.nextSteps.map((step, i) => (
                  <ListItem key={i} disableGutters>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <Typography variant="body2" color="text.secondary">
                        {i + 1}.
                      </Typography>
                    </ListItemIcon>
                    <ListItemText primary={step} />
                  </ListItem>
                ))}
              </List>
            )}
          </AccordionDetails>
        </Accordion>

        {/* Last Updated */}
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
          Last updated: {new Date(context.lastUpdated).toLocaleString()}
        </Typography>
      </CardContent>
    </Card>
  );
}
