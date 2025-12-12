import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  LinearProgress,
  Collapse,
  IconButton,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  AccountTree as DecisionIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  RadioButtonUnchecked as OptionIcon,
  CheckCircle as ChosenIcon,
} from '@mui/icons-material';
import { useState } from 'react';
import type { ReasoningLog } from '../../types';

interface ReasoningLogCardProps {
  log: ReasoningLog;
}

export default function ReasoningLogCard({ log }: ReasoningLogCardProps) {
  const [expanded, setExpanded] = useState(false);

  const confidenceColor = log.confidencePercent
    ? log.confidencePercent >= 80
      ? 'success'
      : log.confidencePercent >= 50
        ? 'warning'
        : 'error'
    : 'info';

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
          <DecisionIcon color="secondary" />
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" fontWeight={600}>
              {log.decisionPoint}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {new Date(log.createdAt).toLocaleString()}
            </Typography>
          </Box>
          {log.confidencePercent !== null && (
            <Chip
              label={`${log.confidencePercent}% confident`}
              size="small"
              color={confidenceColor}
            />
          )}
          <IconButton size="small" onClick={() => setExpanded(!expanded)}>
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>

        {/* Confidence Bar */}
        {log.confidencePercent !== null && (
          <Box sx={{ mt: 2 }}>
            <LinearProgress
              variant="determinate"
              value={log.confidencePercent}
              color={confidenceColor}
              sx={{ height: 6, borderRadius: 3 }}
            />
          </Box>
        )}

        {/* Chosen Option */}
        <Box sx={{ mt: 2, p: 1.5, bgcolor: 'success.light', borderRadius: 1, opacity: 0.9 }}>
          <Typography variant="body2" fontWeight={500} color="success.contrastText">
            Chosen: {log.chosenOption}
          </Typography>
        </Box>

        {/* Rationale */}
        <Box sx={{ mt: 2 }}>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Rationale
          </Typography>
          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
            {log.rationale}
          </Typography>
        </Box>

        {/* Expanded: Options Considered */}
        <Collapse in={expanded}>
          {log.optionsConsidered && log.optionsConsidered.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Options Considered
              </Typography>
              <List dense disablePadding>
                {log.optionsConsidered.map((option, index) => {
                  const isChosen = option === log.chosenOption;
                  return (
                    <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                      <ListItemIcon sx={{ minWidth: 32 }}>
                        {isChosen ? (
                          <ChosenIcon fontSize="small" color="success" />
                        ) : (
                          <OptionIcon fontSize="small" color="action" />
                        )}
                      </ListItemIcon>
                      <ListItemText
                        primary={option}
                        primaryTypographyProps={{
                          variant: 'body2',
                          fontWeight: isChosen ? 600 : 400,
                          color: isChosen ? 'success.main' : 'text.primary',
                        }}
                      />
                    </ListItem>
                  );
                })}
              </List>
            </Box>
          )}
        </Collapse>
      </CardContent>
    </Card>
  );
}
