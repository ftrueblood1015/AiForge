import {
  Card,
  CardContent,
  Typography,
  Box,
  IconButton,
  Tooltip,
  Chip,
  Switch,
} from '@mui/material';
import {
  MoreVert as MoreIcon,
  Business as OrgIcon,
  Folder as ProjectIcon,
} from '@mui/icons-material';
import type { AgentListItem } from '../../types';
import AgentStatusChip from './AgentStatusChip';
import AgentTypeChip from './AgentTypeChip';

interface AgentCardProps {
  agent: AgentListItem;
  onClick?: (agent: AgentListItem) => void;
  onMenuClick?: (event: React.MouseEvent, agent: AgentListItem) => void;
  onToggleEnabled?: (agent: AgentListItem, enabled: boolean) => void;
}

export default function AgentCard({
  agent,
  onClick,
  onMenuClick,
  onToggleEnabled,
}: AgentCardProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(agent);
    }
  };

  const handleMenuClick = (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onMenuClick) {
      onMenuClick(event, agent);
    }
  };

  const handleToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    event.stopPropagation();
    if (onToggleEnabled) {
      onToggleEnabled(agent, event.target.checked);
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
        opacity: agent.isEnabled ? 1 : 0.7,
      }}
      onClick={handleClick}
    >
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AgentTypeChip agentType={agent.agentType} />
            <Typography variant="caption" color="text.secondary" fontWeight={500}>
              {agent.agentKey}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            {onToggleEnabled && (
              <Tooltip title={agent.isEnabled ? 'Disable agent' : 'Enable agent'}>
                <Switch
                  size="small"
                  checked={agent.isEnabled}
                  onChange={handleToggle}
                  onClick={(e) => e.stopPropagation()}
                />
              </Tooltip>
            )}
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
          {agent.name}
        </Typography>

        {/* Description */}
        {agent.description && (
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
            {agent.description}
          </Typography>
        )}

        {/* Footer */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
          <AgentStatusChip status={agent.status} />
          <Chip
            icon={agent.scope === 'Organization' ? <OrgIcon fontSize="small" /> : <ProjectIcon fontSize="small" />}
            label={agent.scope}
            size="small"
            variant="outlined"
            sx={{ height: 24, fontSize: '0.7rem' }}
          />
        </Box>
      </CardContent>
    </Card>
  );
}
