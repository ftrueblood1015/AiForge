import {
  Card,
  CardContent,
  Typography,
  Box,
  IconButton,
  Tooltip,
  Chip,
} from '@mui/material';
import {
  MoreVert as MoreIcon,
  CheckCircle as PublishedIcon,
  Cancel as UnpublishedIcon,
  Business as OrgIcon,
  Folder as ProjectIcon,
  Link as LinkIcon,
  PlayArrow as ExecutionIcon,
} from '@mui/icons-material';
import type { SkillChainSummary } from '../../types';

interface ChainCardProps {
  chain: SkillChainSummary;
  onClick?: (chain: SkillChainSummary) => void;
  onMenuClick?: (event: React.MouseEvent, chain: SkillChainSummary) => void;
}

export default function ChainCard({
  chain,
  onClick,
  onMenuClick,
}: ChainCardProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(chain);
    }
  };

  const handleMenuClick = (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onMenuClick) {
      onMenuClick(event, chain);
    }
  };

  return (
    <Card
      sx={{
        cursor: onClick ? 'pointer' : 'default',
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': onClick
          ? {
              boxShadow: 3,
              transform: 'translateY(-2px)',
            }
          : {},
        opacity: chain.isPublished ? 1 : 0.7,
      }}
      onClick={handleClick}
    >
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Chip
              label="Chain"
              size="small"
              color="primary"
              variant="outlined"
              sx={{ height: 20, fontSize: '0.65rem', fontWeight: 600 }}
            />
            <Typography
              variant="caption"
              color="text.secondary"
              fontWeight={500}
              sx={{ fontFamily: 'monospace' }}
            >
              {chain.chainKey}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Tooltip title={chain.isPublished ? 'Published' : 'Not published'}>
              {chain.isPublished ? (
                <PublishedIcon color="success" fontSize="small" />
              ) : (
                <UnpublishedIcon color="disabled" fontSize="small" />
              )}
            </Tooltip>
            {onMenuClick && (
              <Tooltip title="More options">
                <IconButton size="small" onClick={handleMenuClick}>
                  <MoreIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        </Box>

        {/* Name */}
        <Typography
          variant="body1"
          fontWeight={500}
          sx={{
            mb: 0.5,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {chain.name}
        </Typography>

        {/* Description */}
        {chain.description && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: 1.5,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
            }}
          >
            {chain.description}
          </Typography>
        )}

        {/* Stats */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
          <Tooltip title="Number of links">
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <LinkIcon fontSize="small" color="action" />
              <Typography variant="caption" color="text.secondary">
                {chain.linkCount} links
              </Typography>
            </Box>
          </Tooltip>
          <Tooltip title="Number of executions">
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <ExecutionIcon fontSize="small" color="action" />
              <Typography variant="caption" color="text.secondary">
                {chain.executionCount} runs
              </Typography>
            </Box>
          </Tooltip>
        </Box>

        {/* Footer */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>
          <Chip
            icon={chain.scope === 'Organization' ? <OrgIcon fontSize="small" /> : <ProjectIcon fontSize="small" />}
            label={chain.scope === 'Organization' ? 'Organization' : chain.projectName || 'Project'}
            size="small"
            variant="outlined"
            sx={{ height: 24, fontSize: '0.7rem' }}
          />
        </Box>
      </CardContent>
    </Card>
  );
}
