import { Chip } from '@mui/material';
import {
  AccountTree as WorkflowIcon,
  Analytics as AnalysisIcon,
  Description as DocIcon,
  AutoAwesome as GenerationIcon,
  Science as TestingIcon,
  Settings as CustomIcon,
} from '@mui/icons-material';
import type { SkillCategory } from '../../types';

const categoryIcons: Record<SkillCategory, React.ReactNode> = {
  Workflow: <WorkflowIcon fontSize="small" />,
  Analysis: <AnalysisIcon fontSize="small" />,
  Documentation: <DocIcon fontSize="small" />,
  Generation: <GenerationIcon fontSize="small" />,
  Testing: <TestingIcon fontSize="small" />,
  Custom: <CustomIcon fontSize="small" />,
};

const categoryColors: Record<SkillCategory, 'default' | 'primary' | 'secondary' | 'success' | 'info' | 'warning'> = {
  Workflow: 'primary',
  Analysis: 'info',
  Documentation: 'secondary',
  Generation: 'warning',
  Testing: 'success',
  Custom: 'default',
};

interface SkillCategoryChipProps {
  category: SkillCategory;
  size?: 'small' | 'medium';
}

export default function SkillCategoryChip({ category, size = 'small' }: SkillCategoryChipProps) {
  return (
    <Chip
      icon={categoryIcons[category] as React.ReactElement}
      label={category}
      size={size}
      color={categoryColors[category]}
      variant="outlined"
    />
  );
}
