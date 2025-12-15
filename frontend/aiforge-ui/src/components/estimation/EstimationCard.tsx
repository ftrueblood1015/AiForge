import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  LinearProgress,
  Tooltip,
} from '@mui/material';
import {
  Speed as ComplexityIcon,
  Schedule as EffortIcon,
  TrendingUp as VarianceIcon,
} from '@mui/icons-material';
import type { EffortEstimation, ComplexityLevel, EffortSize } from '../../types';

interface EstimationCardProps {
  estimation: EffortEstimation;
  showVariance?: boolean;
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
    return { label: `+${diff} size${diff > 1 ? 's' : ''} over`, color: 'error' };
  } else {
    return { label: `${Math.abs(diff)} size${Math.abs(diff) > 1 ? 's' : ''} under`, color: 'success' };
  }
}

export default function EstimationCard({ estimation, showVariance = true }: EstimationCardProps) {
  const hasActual = estimation.actualEffort !== null;
  const variance = hasActual
    ? getVarianceIndicator(estimation.estimatedEffort, estimation.actualEffort!)
    : null;

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        {/* Header with version */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary">
            {estimation.isLatest ? 'Current Estimate' : `Version ${estimation.version}`}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {new Date(estimation.createdAt).toLocaleString()}
          </Typography>
        </Box>

        {/* Main estimation badges */}
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
          <Tooltip title="Complexity Assessment">
            <Chip
              icon={<ComplexityIcon />}
              label={complexityLabels[estimation.complexity]}
              color={complexityColors[estimation.complexity]}
              size="small"
            />
          </Tooltip>
          <Tooltip title={`Estimated Effort: ${effortFullLabels[estimation.estimatedEffort]}`}>
            <Chip
              icon={<EffortIcon />}
              label={`Est: ${effortLabels[estimation.estimatedEffort]}`}
              color={effortColors[estimation.estimatedEffort]}
              size="small"
              variant="outlined"
            />
          </Tooltip>
          {hasActual && (
            <Tooltip title={`Actual Effort: ${effortFullLabels[estimation.actualEffort!]}`}>
              <Chip
                icon={<EffortIcon />}
                label={`Act: ${effortLabels[estimation.actualEffort!]}`}
                color={effortColors[estimation.actualEffort!]}
                size="small"
              />
            </Tooltip>
          )}
        </Box>

        {/* Confidence meter */}
        <Box sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
            <Typography variant="body2" color="text.secondary">
              Confidence
            </Typography>
            <Chip
              label={`${estimation.confidencePercent}%`}
              size="small"
              color={getConfidenceColor(estimation.confidencePercent)}
            />
          </Box>
          <LinearProgress
            variant="determinate"
            value={estimation.confidencePercent}
            color={getConfidenceColor(estimation.confidencePercent)}
            sx={{ height: 6, borderRadius: 3 }}
          />
        </Box>

        {/* Variance indicator */}
        {showVariance && hasActual && variance && (
          <Box sx={{ mb: 2, p: 1.5, bgcolor: `${variance.color}.light`, borderRadius: 1, opacity: 0.9 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <VarianceIcon fontSize="small" />
              <Typography variant="body2" fontWeight={500}>
                {variance.label}
              </Typography>
            </Box>
            {estimation.varianceNotes && (
              <Typography variant="body2" sx={{ mt: 1, color: 'text.secondary' }}>
                {estimation.varianceNotes}
              </Typography>
            )}
          </Box>
        )}

        {/* Reasoning */}
        {estimation.estimationReasoning && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Reasoning
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
              {estimation.estimationReasoning}
            </Typography>
          </Box>
        )}

        {/* Assumptions */}
        {estimation.assumptions && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Assumptions
            </Typography>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
              {estimation.assumptions}
            </Typography>
          </Box>
        )}

        {/* Revision reason (for non-latest versions) */}
        {estimation.revisionReason && (
          <Box
            sx={{
              mt: 2,
              p: 1.5,
              bgcolor: 'warning.light',
              borderRadius: 1,
              opacity: 0.8,
            }}
          >
            <Typography variant="body2" fontWeight={500}>
              Revised: {estimation.revisionReason}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
}
