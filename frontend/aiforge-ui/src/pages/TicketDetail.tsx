import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Chip,
  Card,
  CardContent,
  Grid,
  Skeleton,
  Alert,
  Breadcrumbs,
  Link,
  Divider,
  Button,
  Menu,
  MenuItem,
  TextField,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
  Edit as EditIcon,
  Send as SendIcon,
  SmartToy as AiIcon,
  Person as PersonIcon,
} from '@mui/icons-material';
import { useTicketStore } from '../stores/ticketStore';
import { ticketsApi } from '../api/tickets';
import type { TicketType, TicketStatus, Priority, Comment } from '../types';

const typeIcons: Record<TicketType, React.ReactNode> = {
  Task: <AssignmentIcon color="action" fontSize="large" />,
  Bug: <BugIcon color="error" fontSize="large" />,
  Feature: <FeatureIcon color="warning" fontSize="large" />,
  Enhancement: <EnhancementIcon color="info" fontSize="large" />,
};

const statusColors: Record<TicketStatus, 'default' | 'primary' | 'warning' | 'success'> = {
  ToDo: 'default',
  InProgress: 'primary',
  InReview: 'warning',
  Done: 'success',
};

const statusLabels: Record<TicketStatus, string> = {
  ToDo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

const priorityColors: Record<Priority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

const STATUSES: TicketStatus[] = ['ToDo', 'InProgress', 'InReview', 'Done'];

export default function TicketDetail() {
  const { key } = useParams<{ key: string }>();
  const navigate = useNavigate();
  const { currentTicket, isLoading, error, fetchTicket, updateTicketStatus } = useTicketStore();

  const [statusMenuAnchor, setStatusMenuAnchor] = useState<null | HTMLElement>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [loadingComments, setLoadingComments] = useState(false);
  const [submittingComment, setSubmittingComment] = useState(false);

  useEffect(() => {
    if (key) {
      fetchTicket(key);
    }
  }, [key, fetchTicket]);

  useEffect(() => {
    if (currentTicket) {
      loadComments();
    }
  }, [currentTicket]);

  const loadComments = async () => {
    if (!currentTicket) return;
    setLoadingComments(true);
    try {
      const data = await ticketsApi.getComments(currentTicket.id);
      setComments(data);
    } catch (err) {
      console.error('Failed to load comments:', err);
    } finally {
      setLoadingComments(false);
    }
  };

  const handleStatusChange = async (newStatus: TicketStatus) => {
    if (!currentTicket) return;
    setStatusMenuAnchor(null);
    await updateTicketStatus(currentTicket.id, newStatus);
    fetchTicket(key!);
  };

  const handleAddComment = async () => {
    if (!newComment.trim() || !currentTicket) return;
    setSubmittingComment(true);
    try {
      await ticketsApi.addComment(currentTicket.id, newComment);
      setNewComment('');
      await loadComments();
    } catch (err) {
      console.error('Failed to add comment:', err);
    } finally {
      setSubmittingComment(false);
    }
  };

  if (error) {
    return (
      <Box>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (isLoading || !currentTicket) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={40} />
        <Skeleton variant="rectangular" height={400} sx={{ mt: 2, borderRadius: 2 }} />
      </Box>
    );
  }

  const projectKey = currentTicket.key.split('-')[0];

  return (
    <Box>
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          component="button"
          variant="body2"
          onClick={() => navigate('/projects')}
          underline="hover"
          color="inherit"
        >
          Projects
        </Link>
        <Link
          component="button"
          variant="body2"
          onClick={() => navigate(`/projects/${projectKey}`)}
          underline="hover"
          color="inherit"
        >
          {projectKey}
        </Link>
        <Typography color="text.primary">{currentTicket.key}</Typography>
      </Breadcrumbs>

      <Grid container spacing={3}>
        {/* Main Content */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              {/* Header */}
              <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2, mb: 3 }}>
                {typeIcons[currentTicket.type]}
                <Box sx={{ flex: 1 }}>
                  <Typography variant="caption" color="text.secondary">
                    {currentTicket.key}
                  </Typography>
                  <Typography variant="h5">{currentTicket.title}</Typography>
                </Box>
                <IconButton>
                  <EditIcon />
                </IconButton>
              </Box>

              {/* Status Chips */}
              <Box sx={{ display: 'flex', gap: 1, mb: 3 }}>
                <Chip
                  label={statusLabels[currentTicket.status]}
                  color={statusColors[currentTicket.status]}
                  onClick={(e) => setStatusMenuAnchor(e.currentTarget)}
                  sx={{ cursor: 'pointer' }}
                />
                <Chip label={currentTicket.type} variant="outlined" />
                <Chip label={currentTicket.priority} color={priorityColors[currentTicket.priority]} />
              </Box>

              {/* Status Change Menu */}
              <Menu
                anchorEl={statusMenuAnchor}
                open={Boolean(statusMenuAnchor)}
                onClose={() => setStatusMenuAnchor(null)}
              >
                {STATUSES.map((status) => (
                  <MenuItem
                    key={status}
                    onClick={() => handleStatusChange(status)}
                    selected={status === currentTicket.status}
                  >
                    <Chip
                      label={statusLabels[status]}
                      size="small"
                      color={statusColors[status]}
                      sx={{ mr: 1 }}
                    />
                    {statusLabels[status]}
                  </MenuItem>
                ))}
              </Menu>

              <Divider sx={{ my: 2 }} />

              {/* Description */}
              <Typography variant="h6" gutterBottom>
                Description
              </Typography>
              <Typography
                variant="body1"
                sx={{ whiteSpace: 'pre-wrap', color: currentTicket.description ? 'text.primary' : 'text.secondary' }}
              >
                {currentTicket.description || 'No description provided.'}
              </Typography>

              <Divider sx={{ my: 3 }} />

              {/* Comments Section */}
              <Typography variant="h6" gutterBottom>
                Comments ({comments.length})
              </Typography>

              {/* Add Comment */}
              <Box sx={{ display: 'flex', gap: 1, mb: 3 }}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Add a comment..."
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && !e.shiftKey && handleAddComment()}
                  multiline
                  maxRows={4}
                />
                <IconButton
                  color="primary"
                  onClick={handleAddComment}
                  disabled={!newComment.trim() || submittingComment}
                >
                  <SendIcon />
                </IconButton>
              </Box>

              {/* Comments List */}
              {loadingComments ? (
                <Box>
                  {[1, 2].map((i) => (
                    <Skeleton key={i} variant="rectangular" height={60} sx={{ mb: 1, borderRadius: 1 }} />
                  ))}
                </Box>
              ) : comments.length === 0 ? (
                <Typography color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
                  No comments yet
                </Typography>
              ) : (
                <List disablePadding>
                  {comments.map((comment) => (
                    <ListItem key={comment.id} alignItems="flex-start" sx={{ px: 0 }}>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: comment.isAiGenerated ? 'secondary.main' : 'primary.main' }}>
                          {comment.isAiGenerated ? <AiIcon /> : <PersonIcon />}
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="body2" fontWeight={500}>
                              {comment.isAiGenerated ? 'Claude' : 'User'}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {new Date(comment.createdAt).toLocaleString()}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 0.5 }}>
                            {comment.content}
                          </Typography>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Sidebar */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Details
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Status
                  </Typography>
                  <Box sx={{ mt: 0.5 }}>
                    <Chip
                      label={statusLabels[currentTicket.status]}
                      color={statusColors[currentTicket.status]}
                      size="small"
                    />
                  </Box>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Type
                  </Typography>
                  <Typography variant="body1">{currentTicket.type}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Priority
                  </Typography>
                  <Box sx={{ mt: 0.5 }}>
                    <Chip
                      label={currentTicket.priority}
                      color={priorityColors[currentTicket.priority]}
                      size="small"
                    />
                  </Box>
                </Box>
                <Divider />
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Created
                  </Typography>
                  <Typography variant="body2">
                    {new Date(currentTicket.createdAt).toLocaleString()}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Updated
                  </Typography>
                  <Typography variant="body2">
                    {new Date(currentTicket.updatedAt).toLocaleString()}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>

          {/* Quick Actions */}
          <Card sx={{ mt: 2 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Quick Actions
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {currentTicket.status !== 'InProgress' && (
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => handleStatusChange('InProgress')}
                  >
                    Start Progress
                  </Button>
                )}
                {currentTicket.status === 'InProgress' && (
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => handleStatusChange('InReview')}
                  >
                    Move to Review
                  </Button>
                )}
                {currentTicket.status === 'InReview' && (
                  <Button
                    variant="outlined"
                    color="success"
                    size="small"
                    onClick={() => handleStatusChange('Done')}
                  >
                    Mark Done
                  </Button>
                )}
                {currentTicket.status === 'Done' && (
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => handleStatusChange('ToDo')}
                  >
                    Reopen
                  </Button>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
