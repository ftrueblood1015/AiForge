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
import {
  Psychology as PsychologyIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  CheckCircle as CheckIcon,
  RadioButtonUnchecked as PendingIcon,
  Lightbulb as AssumptionIcon,
} from '@mui/icons-material';
import { useState } from 'react';
import type { PlanningSession } from '../../types';

interface PlanningSessionCardProps {
  session: PlanningSession;
}

export default function PlanningSessionCard({ session }: PlanningSessionCardProps) {
  const [expanded, setExpanded] = useState(false);
  const isCompleted = !!session.completedAt;

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
          <PsychologyIcon color={isCompleted ? 'success' : 'primary'} />
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                Planning Session
              </Typography>
              <Chip
                label={isCompleted ? 'Completed' : 'In Progress'}
                size="small"
                color={isCompleted ? 'success' : 'warning'}
                icon={isCompleted ? <CheckIcon /> : <PendingIcon />}
              />
            </Box>
            <Typography variant="caption" color="text.secondary">
              {new Date(session.createdAt).toLocaleString()}
              {isCompleted && ` - ${new Date(session.completedAt!).toLocaleString()}`}
            </Typography>
          </Box>
          <IconButton size="small" onClick={() => setExpanded(!expanded)}>
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>

        {/* Initial Understanding */}
        <Box sx={{ mt: 2 }}>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Initial Understanding
          </Typography>
          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
            {session.initialUnderstanding}
          </Typography>
        </Box>

        {/* Expanded Content */}
        <Collapse in={expanded}>
          {/* Assumptions */}
          {session.assumptions && session.assumptions.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Assumptions
              </Typography>
              <List dense disablePadding>
                {session.assumptions.map((assumption, index) => (
                  <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <AssumptionIcon fontSize="small" color="info" />
                    </ListItemIcon>
                    <ListItemText primary={assumption} primaryTypographyProps={{ variant: 'body2' }} />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}

          {/* Chosen Approach (if completed) */}
          {isCompleted && session.chosenApproach && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Chosen Approach
              </Typography>
              <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                {session.chosenApproach}
              </Typography>
            </Box>
          )}

          {/* Rationale (if completed) */}
          {isCompleted && session.rationale && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Rationale
              </Typography>
              <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                {session.rationale}
              </Typography>
            </Box>
          )}
        </Collapse>
      </CardContent>
    </Card>
  );
}
