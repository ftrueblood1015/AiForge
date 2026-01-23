import { Chip } from '@mui/material';
import {
  SmartToy as ClaudeIcon,
  Psychology as GPTIcon,
  AutoAwesome as GeminiIcon,
  Settings as CustomIcon,
} from '@mui/icons-material';
import type { AgentType } from '../../types';

const typeIcons: Record<AgentType, React.ReactNode> = {
  Claude: <ClaudeIcon fontSize="small" />,
  GPT: <GPTIcon fontSize="small" />,
  Gemini: <GeminiIcon fontSize="small" />,
  Custom: <CustomIcon fontSize="small" />,
};

const typeColors: Record<AgentType, 'default' | 'primary' | 'secondary' | 'success' | 'info'> = {
  Claude: 'primary',
  GPT: 'success',
  Gemini: 'info',
  Custom: 'default',
};

interface AgentTypeChipProps {
  agentType: AgentType;
  size?: 'small' | 'medium';
}

export default function AgentTypeChip({ agentType, size = 'small' }: AgentTypeChipProps) {
  return (
    <Chip
      icon={typeIcons[agentType] as React.ReactElement}
      label={agentType}
      size={size}
      color={typeColors[agentType]}
      variant="outlined"
    />
  );
}
