import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Chip,
  LinearProgress,
  Skeleton,
  Alert,
  Tooltip,
  IconButton,
  Collapse,
  Divider,
} from '@mui/material';
import {
  Speed as ComplexityIcon,
  Schedule as EffortIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Assessment as EstimationIcon,
  TrendingUp as VarianceIcon,
} from '@mui/icons-material';
import { estimationApi } from '../../api/estimation';
import type { EffortEstimation, ComplexityLevel, EffortSize } from '../../types';

interface EstimationSectionProps {
  ticketId: string;
}

const complexityColors: Record<ComplexityLevel, 'success' | 'info' | 'warning' | 'error'> = {
  Low: 'success',
  Medium: 'info',
  High: 'warning',
  VeryHigh: 'error',
};

const complexityLabels: Record<ComplexityLevel, string> = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  VeryHigh: 'Very High',
};

const effortColors: Record<EffortSize, 'success' | 'info' | 'primary' | 'warning' | 'error'> = {
  XSmall: 'success',
  Small: 'success',
  Medium: 'info',
  Large: 'warning',
  XLarge: 'error',
};

const effortLabels: Record<EffortSize, string> = {
  XSmall: 'XS',
  Small: 'S',
  Medium: 'M',
  Large: 'L',
  XLarge: 'XL',
};

const effortFullLabels: Record<EffortSize, string> = {
  XSmall: 'Extra Small',
  Small: 'Small',
  Medium: 'Medium',
  Large: 'Large',
  XLarge: 'Extra Large',
};

function getConfidenceColor(confidence: number): 'success' | 'warning' | 'error' {
  if (confidence >= 80) return 'success';
  if (confidence >= 50) return 'warning';
  return 'error';
}

function getVarianceIndicator(estimated: EffortSize, actual: EffortSize): {
  label: string;
  color: 'success' | 'warning' | 'error';
} {
  const effortOrder: EffortSize[] = ['XSmall', 'Small', 'Medium', 'Large', 'XLarge'];
  const estimatedIndex = effortOrder.indexOf(estimated);
  const actualIndex = effortOrder.indexOf(actual);
  const diff = actualIndex - estimatedIndex;

  if (diff === 0) {
    return { label: 'On Target', color: 'success' };
  } else if (diff > 0) {
    return { label: `+${diff} over`, color: 'error' };
  } else {
    return { label: `${Math.abs(diff)} under`, color: 'success' };
  }
}

export default function EstimationSection({ ticketId }: EstimationSectionProps) {
  const [estimation, setEstimation] = useState<EffortEstimation | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expanded, setExpanded] = useState(false);

  useEffect(() => {
    loadEstimation();
  }, [ticketId]);

  const loadEstimation = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await estimationApi.getLatest(ticketId);
      setEstimation(data);
    } catch (err) {
      setError('Failed to load estimation');
      console.error('Error loading estimation:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box>
        <Skeleton variant="text" width={100} height={24} />
        <Skeleton variant="rectangular" height={80} sx={{ mt: 1, borderRadius: 1 }} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error" sx={{ py: 0.5 }}>{error}</Alert>;
  }

  if (!estimation) {
    return (
      <Box sx={{ textAlign: 'center', py: 2, color: 'text.secondary' }}>
        <EstimationIcon sx={{ fontSize: 32, opacity: 0.5, mb: 1 }} />
        <Typography variant="body2">No estimation yet</Typography>
      </Box>
    );
  }

  const hasActual = estimation.actualEffort !== null;
  const variance = hasActual
    ? getVarianceIndicator(estimation.estimatedEffort, estimation.actualEffort!)
    : null;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
        <Typography variant="h6">Estimation</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          {estimation.version > 1 && (
            <Chip label={`v${estimation.version}`} size="small" variant="outlined" />
          )}
          <IconButton size="small" onClick={() => setExpanded(!expanded)}>
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
      </Box>

      {/* Compact view: badges */}
      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 1.5 }}>
        <Tooltip title={`Complexity: ${complexityLabels[estimation.complexity]}`}>
          <Chip
            icon={<ComplexityIcon />}
            label={complexityLabels[estimation.complexity]}
            color={complexityColors[estimation.complexity]}
            size="small"
          />
        </Tooltip>
        <Tooltip title={`Estimated: ${effortFullLabels[estimation.estimatedEffort]}`}>
          <Chip
            icon={<EffortIcon />}
            label={effortLabels[estimation.estimatedEffort]}
            color={effortColors[estimation.estimatedEffort]}
            size="small"
            variant="outlined"
          />
        </Tooltip>
        {hasActual && (
          <Tooltip title={`Actual: ${effortFullLabels[estimation.actualEffort!]}`}>
            <Chip
              label={`Act: ${effortLabels[estimation.actualEffort!]}`}
              color={effortColors[estimation.actualEffort!]}
              size="small"
            />
          </Tooltip>
        )}
      </Box>

      {/* Confidence bar */}
      <Box sx={{ mb: 1 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
          <Typography variant="caption" color="text.secondary">
            Confidence
          </Typography>
          <Typography variant="caption" fontWeight={600} color={`${getConfidenceColor(estimation.confidencePercent)}.main`}>
            {estimation.confidencePercent}%
          </Typography>
        </Box>
        <LinearProgress
          variant="determinate"
          value={estimation.confidencePercent}
          color={getConfidenceColor(estimation.confidencePercent)}
          sx={{ height: 4, borderRadius: 2 }}
        />
      </Box>

      {/* Variance indicator */}
      {hasActual && variance && (
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 0.5,
            p: 1,
            bgcolor: `${variance.color}.light`,
            borderRadius: 1,
            mb: 1,
          }}
        >
          <VarianceIcon fontSize="small" />
          <Typography variant="body2" fontWeight={500}>
            {variance.label}
          </Typography>
        </Box>
      )}

      {/* Expanded details */}
      <Collapse in={expanded}>
        <Divider sx={{ my: 1.5 }} />

        {estimation.estimationReasoning && (
          <Box sx={{ mb: 1.5 }}>
            <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
              Reasoning
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
              {estimation.estimationReasoning}
            </Typography>
          </Box>
        )}

        {estimation.assumptions && (
          <Box sx={{ mb: 1.5 }}>
            <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
              Assumptions
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
              {estimation.assumptions}
            </Typography>
          </Box>
        )}

        {estimation.varianceNotes && (
          <Box sx={{ mb: 1.5 }}>
            <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
              Variance Notes
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
              {estimation.varianceNotes}
            </Typography>
          </Box>
        )}

        <Typography variant="caption" color="text.secondary">
          Created: {new Date(estimation.createdAt).toLocaleString()}
        </Typography>
      </Collapse>
    </Box>
  );
}
