import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  Collapse,
  IconButton,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { useCompactMarkdownStyles } from '../../hooks';
import {
  PlayArrow as ProgressIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  CheckCircle as SuccessIcon,
  Cancel as FailureIcon,
  Warning as PartialIcon,
  Block as BlockedIcon,
  InsertDriveFile as FileIcon,
} from '@mui/icons-material';
import { useState } from 'react';
import type { ProgressEntry, ProgressOutcome } from '../../types';

interface ProgressEntryCardProps {
  entry: ProgressEntry;
}

const outcomeConfig: Record<ProgressOutcome, { icon: React.ReactNode; color: 'success' | 'error' | 'warning' | 'default'; label: string }> = {
  Success: { icon: <SuccessIcon />, color: 'success', label: 'Success' },
  Failure: { icon: <FailureIcon />, color: 'error', label: 'Failed' },
  Partial: { icon: <PartialIcon />, color: 'warning', label: 'Partial' },
  Blocked: { icon: <BlockedIcon />, color: 'default', label: 'Blocked' },
};

export default function ProgressEntryCard({ entry }: ProgressEntryCardProps) {
  const [expanded, setExpanded] = useState(false);
  const compactMarkdownStyles = useCompactMarkdownStyles();
  const config = outcomeConfig[entry.outcome];

  const hasDetails = (entry.filesAffected && entry.filesAffected.length > 0) || entry.errorDetails;

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
          <ProgressIcon color={config.color === 'default' ? 'action' : config.color} />
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                Progress Update
              </Typography>
              <Chip
                label={config.label}
                size="small"
                color={config.color}
                icon={config.icon as React.ReactElement}
              />
            </Box>
            <Typography variant="caption" color="text.secondary">
              {new Date(entry.createdAt).toLocaleString()}
            </Typography>
          </Box>
          {hasDetails && (
            <IconButton size="small" onClick={() => setExpanded(!expanded)}>
              {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            </IconButton>
          )}
        </Box>

        {/* Content */}
        <Box sx={{ mt: 2, ...compactMarkdownStyles }}>
          <ReactMarkdown remarkPlugins={[remarkGfm]}>{entry.content}</ReactMarkdown>
        </Box>

        {/* Expanded Content */}
        <Collapse in={expanded}>
          {/* Files Affected */}
          {entry.filesAffected && entry.filesAffected.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Files Affected
              </Typography>
              <List dense disablePadding>
                {entry.filesAffected.map((file, index) => (
                  <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <FileIcon fontSize="small" color="action" />
                    </ListItemIcon>
                    <ListItemText
                      primary={file}
                      primaryTypographyProps={{
                        variant: 'body2',
                        fontFamily: 'monospace',
                        fontSize: '0.8rem',
                      }}
                    />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}

          {/* Error Details */}
          {entry.errorDetails && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Error Details
              </Typography>
              <Box
                sx={{
                  p: 1.5,
                  bgcolor: 'error.light',
                  borderRadius: 1,
                  fontFamily: 'monospace',
                  fontSize: '0.8rem',
                  whiteSpace: 'pre-wrap',
                  color: 'error.contrastText',
                }}
              >
                {entry.errorDetails}
              </Box>
            </Box>
          )}
        </Collapse>
      </CardContent>
    </Card>
  );
}
