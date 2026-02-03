import {
  Avatar,
  Box,
  Chip,
  IconButton,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Menu,
  MenuItem,
  Typography,
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import { useState } from 'react';
import type { ProjectMember, ProjectRole } from '../../types';

interface MemberListItemProps {
  member: ProjectMember;
  isOwner: boolean;
  canModify: boolean;
  onRoleChange?: (userId: string, newRole: ProjectRole) => void;
  onRemove?: (userId: string) => void;
}

const roleColors: Record<ProjectRole, 'primary' | 'default' | 'secondary'> = {
  Owner: 'primary',
  Member: 'default',
  Viewer: 'secondary',
};

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((part) => part[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

export default function MemberListItem({
  member,
  isOwner,
  canModify,
  onRoleChange,
  onRemove,
}: MemberListItemProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleRoleChange = (role: ProjectRole) => {
    handleMenuClose();
    onRoleChange?.(member.userId, role);
  };

  const handleRemove = () => {
    handleMenuClose();
    onRemove?.(member.userId);
  };

  const showActions = canModify && (onRoleChange || onRemove);

  return (
    <ListItem
      sx={{
        '&:hover': { bgcolor: 'action.hover' },
        borderRadius: 1,
      }}
      secondaryAction={
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Chip
            label={member.role}
            size="small"
            color={roleColors[member.role]}
            variant={member.role === 'Owner' ? 'filled' : 'outlined'}
          />
          {showActions && (
            <>
              <IconButton size="small" onClick={handleMenuClick}>
                <MoreVertIcon fontSize="small" />
              </IconButton>
              <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleMenuClose}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                transformOrigin={{ vertical: 'top', horizontal: 'right' }}
              >
                {onRoleChange && member.role !== 'Owner' && (
                  <MenuItem onClick={() => handleRoleChange('Owner')}>
                    Make Owner
                  </MenuItem>
                )}
                {onRoleChange && member.role !== 'Member' && (
                  <MenuItem onClick={() => handleRoleChange('Member')}>
                    Set as Member
                  </MenuItem>
                )}
                {onRoleChange && member.role !== 'Viewer' && (
                  <MenuItem onClick={() => handleRoleChange('Viewer')}>
                    Set as Viewer
                  </MenuItem>
                )}
                {onRemove && (
                  <MenuItem onClick={handleRemove} sx={{ color: 'error.main' }}>
                    Remove from project
                  </MenuItem>
                )}
              </Menu>
            </>
          )}
        </Box>
      }
    >
      <ListItemAvatar>
        <Avatar sx={{ bgcolor: 'primary.main', width: 36, height: 36 }}>
          {getInitials(member.displayName || member.email)}
        </Avatar>
      </ListItemAvatar>
      <ListItemText
        primary={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="body1">{member.displayName}</Typography>
            {isOwner && (
              <Typography variant="caption" color="text.secondary">
                (you)
              </Typography>
            )}
          </Box>
        }
        secondary={member.email}
      />
    </ListItem>
  );
}
