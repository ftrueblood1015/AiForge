import {
  Box,
  Typography,
  IconButton,
  Tooltip,
  Paper,
} from '@mui/material';
import {
  DragIndicator as DragIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  SmartToy as AgentIcon,
  Psychology as SkillIcon,
} from '@mui/icons-material';
import type { SkillChainLink } from '../../types';
import TransitionTypeChip from './TransitionTypeChip';

interface ChainLinkItemProps {
  link: SkillChainLink;
  isActive?: boolean;
  onEdit?: (link: SkillChainLink) => void;
  onDelete?: (link: SkillChainLink) => void;
  isDragging?: boolean;
}

export default function ChainLinkItem({
  link,
  isActive = false,
  onEdit,
  onDelete,
  isDragging = false,
}: ChainLinkItemProps) {
  return (
    <Paper
      elevation={isDragging ? 4 : isActive ? 2 : 1}
      sx={{
        p: 2,
        borderLeft: isActive ? '4px solid' : '4px solid transparent',
        borderLeftColor: isActive ? 'primary.main' : 'transparent',
        bgcolor: isActive ? 'action.selected' : 'background.paper',
        transition: 'all 0.2s',
        opacity: isDragging ? 0.8 : 1,
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
        {/* Drag Handle */}
        <Box sx={{ cursor: 'grab', color: 'text.disabled', mt: 0.5 }}>
          <DragIcon fontSize="small" />
        </Box>

        {/* Position Badge */}
        <Box
          sx={{
            width: 28,
            height: 28,
            borderRadius: '50%',
            bgcolor: isActive ? 'primary.main' : 'grey.300',
            color: isActive ? 'primary.contrastText' : 'text.secondary',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontWeight: 600,
            fontSize: '0.875rem',
            flexShrink: 0,
          }}
        >
          {link.position + 1}
        </Box>

        {/* Content */}
        <Box sx={{ flex: 1, minWidth: 0 }}>
          {/* Link Name */}
          <Typography variant="subtitle2" fontWeight={600}>
            {link.name}
          </Typography>

          {/* Description */}
          {link.description && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {link.description}
            </Typography>
          )}

          {/* Skill & Agent Info */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 1, flexWrap: 'wrap' }}>
            <Tooltip title="Skill">
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <SkillIcon fontSize="small" color="primary" />
                <Typography variant="caption" color="text.secondary">
                  {link.skillName || link.skillKey || 'Unknown skill'}
                </Typography>
              </Box>
            </Tooltip>
            {link.agentId && (
              <Tooltip title="Agent">
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                  <AgentIcon fontSize="small" color="secondary" />
                  <Typography variant="caption" color="text.secondary">
                    {link.agentName || link.agentKey || 'Unknown agent'}
                  </Typography>
                </Box>
              </Tooltip>
            )}
            <Typography variant="caption" color="text.disabled">
              Max retries: {link.maxRetries}
            </Typography>
          </Box>

          {/* Transitions */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1, flexWrap: 'wrap' }}>
            <Typography variant="caption" color="text.secondary" sx={{ minWidth: 60 }}>
              On success:
            </Typography>
            <TransitionTypeChip transition={link.onSuccessTransition} />
            {link.onSuccessTargetLinkId && (
              <Typography variant="caption" color="text.disabled">
                → Target link
              </Typography>
            )}
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5, flexWrap: 'wrap' }}>
            <Typography variant="caption" color="text.secondary" sx={{ minWidth: 60 }}>
              On failure:
            </Typography>
            <TransitionTypeChip transition={link.onFailureTransition} />
            {link.onFailureTargetLinkId && (
              <Typography variant="caption" color="text.disabled">
                → Target link
              </Typography>
            )}
          </Box>
        </Box>

        {/* Actions */}
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          {onEdit && (
            <Tooltip title="Edit link">
              <IconButton size="small" onClick={() => onEdit(link)}>
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {onDelete && (
            <Tooltip title="Delete link">
              <IconButton size="small" color="error" onClick={() => onDelete(link)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      </Box>
    </Paper>
  );
}
