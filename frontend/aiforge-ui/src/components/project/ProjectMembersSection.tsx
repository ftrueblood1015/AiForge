import {
  Alert,
  Box,
  Button,
  Collapse,
  IconButton,
  List,
  Paper,
  Skeleton,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import GroupIcon from '@mui/icons-material/Group';
import { useEffect, useState } from 'react';
import type { ProjectRole } from '../../types';
import { useProjectMemberStore } from '../../stores/projectMemberStore';
import { useAuthStore } from '../../stores/authStore';
import MemberListItem from './MemberListItem';
import AddMemberDialog from './AddMemberDialog';

interface ProjectMembersSectionProps {
  projectId: string;
}

export default function ProjectMembersSection({ projectId }: ProjectMembersSectionProps) {
  const [expanded, setExpanded] = useState(true);
  const [addDialogOpen, setAddDialogOpen] = useState(false);

  const { user } = useAuthStore();
  const {
    members,
    currentUserRole,
    isLoading,
    error,
    fetchMembers,
    fetchMyMembership,
    addMember,
    updateMemberRole,
    removeMember,
    clearError,
  } = useProjectMemberStore();

  const isOwner = currentUserRole === 'Owner';

  useEffect(() => {
    fetchMembers(projectId);
    fetchMyMembership(projectId);
  }, [projectId, fetchMembers, fetchMyMembership]);

  const handleAddMember = async (email: string, role: ProjectRole) => {
    await addMember(projectId, email, role);
  };

  const handleRoleChange = async (userId: string, newRole: ProjectRole) => {
    await updateMemberRole(projectId, userId, newRole);
  };

  const handleRemove = async (userId: string) => {
    if (window.confirm('Are you sure you want to remove this member from the project?')) {
      await removeMember(projectId, userId);
    }
  };

  // Count owners for last-owner protection
  const ownerCount = members.filter((m) => m.role === 'Owner').length;

  return (
    <Paper sx={{ mb: 3 }}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          p: 2,
          cursor: 'pointer',
          '&:hover': { bgcolor: 'action.hover' },
        }}
        onClick={() => setExpanded(!expanded)}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <GroupIcon color="action" />
          <Typography variant="h6">
            Members ({members.length})
          </Typography>
          {currentUserRole && (
            <Typography variant="body2" color="text.secondary">
              You are {currentUserRole === 'Owner' ? 'an' : 'a'} {currentUserRole}
            </Typography>
          )}
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {isOwner && (
            <Button
              size="small"
              startIcon={<PersonAddIcon />}
              onClick={(e) => {
                e.stopPropagation();
                setAddDialogOpen(true);
              }}
            >
              Add Member
            </Button>
          )}
          <IconButton size="small">
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
      </Box>

      <Collapse in={expanded}>
        <Box sx={{ px: 2, pb: 2 }}>
          {error && (
            <Alert severity="error" onClose={clearError} sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {isLoading ? (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
              ))}
            </Box>
          ) : members.length === 0 ? (
            <Typography color="text.secondary" sx={{ py: 2 }}>
              No members yet. Add the first member to this project.
            </Typography>
          ) : (
            <List disablePadding>
              {members.map((member) => {
                const isSelf = user?.id === member.userId;
                const isLastOwner = member.role === 'Owner' && ownerCount <= 1;
                // Can modify if owner, not self (unless there are multiple owners), and not last owner
                const canModify = isOwner && !isLastOwner && !(isSelf && member.role === 'Owner' && ownerCount <= 1);

                return (
                  <MemberListItem
                    key={member.id}
                    member={member}
                    isOwner={isSelf}
                    canModify={canModify}
                    onRoleChange={canModify ? handleRoleChange : undefined}
                    onRemove={canModify && !isSelf ? handleRemove : undefined}
                  />
                );
              })}
            </List>
          )}
        </Box>
      </Collapse>

      <AddMemberDialog
        open={addDialogOpen}
        projectId={projectId}
        onClose={() => setAddDialogOpen(false)}
        onAdd={handleAddMember}
      />
    </Paper>
  );
}
