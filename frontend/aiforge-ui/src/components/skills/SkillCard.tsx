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
} from '@mui/icons-material';
import type { SkillListItem } from '../../types';
import SkillCategoryChip from './SkillCategoryChip';

interface SkillCardProps {
  skill: SkillListItem;
  onClick?: (skill: SkillListItem) => void;
  onMenuClick?: (event: React.MouseEvent, skill: SkillListItem) => void;
}

export default function SkillCard({
  skill,
  onClick,
  onMenuClick,
}: SkillCardProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(skill);
    }
  };

  const handleMenuClick = (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onMenuClick) {
      onMenuClick(event, skill);
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
        opacity: skill.isPublished ? 1 : 0.7,
      }}
      onClick={handleClick}
    >
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <SkillCategoryChip category={skill.category} />
            <Typography
              variant="caption"
              color="text.secondary"
              fontWeight={500}
              sx={{ fontFamily: 'monospace' }}
            >
              /{skill.skillKey}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Tooltip title={skill.isPublished ? 'Published' : 'Not published'}>
              {skill.isPublished ? (
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
          {skill.name}
        </Typography>

        {/* Description */}
        {skill.description && (
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
            {skill.description}
          </Typography>
        )}

        {/* Footer */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>
          <Chip
            icon={skill.scope === 'Organization' ? <OrgIcon fontSize="small" /> : <ProjectIcon fontSize="small" />}
            label={skill.scope === 'Organization' ? 'Organization' : skill.projectName || 'Project'}
            size="small"
            variant="outlined"
            sx={{ height: 24, fontSize: '0.7rem' }}
          />
        </Box>
      </CardContent>
    </Card>
  );
}
