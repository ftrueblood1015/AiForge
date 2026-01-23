import {
  ToggleButtonGroup,
  ToggleButton,
  Box,
  Typography,
  Tooltip,
} from '@mui/material';
import {
  LooksOne as Tier1Icon,
  LooksTwo as Tier2Icon,
  Looks3 as Tier3Icon,
  Looks4 as Tier4Icon,
} from '@mui/icons-material';

interface TierSelectorProps {
  value: number;
  onChange: (tier: number) => void;
  disabled?: boolean;
  estimatedTokens?: number;
}

const tierInfo = [
  { value: 1, icon: <Tier1Icon />, label: 'Tier 1', tokens: '~500', description: 'Focus + Context Helper' },
  { value: 2, icon: <Tier2Icon />, label: 'Tier 2', tokens: '~1,500', description: '+ Implementation Plan' },
  { value: 3, icon: <Tier3Icon />, label: 'Tier 3', tokens: '~3,000', description: '+ Item Details' },
  { value: 4, icon: <Tier4Icon />, label: 'Tier 4', tokens: '~5,000+', description: '+ File Snapshots' },
];

export default function TierSelector({
  value,
  onChange,
  disabled = false,
  estimatedTokens,
}: TierSelectorProps) {
  const handleChange = (_event: React.MouseEvent<HTMLElement>, newTier: number | null) => {
    if (newTier !== null) {
      onChange(newTier);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Context Tier
        </Typography>
        {estimatedTokens !== undefined && (
          <Typography variant="caption" color="text.secondary">
            ~{estimatedTokens.toLocaleString()} tokens
          </Typography>
        )}
      </Box>
      <ToggleButtonGroup
        value={value}
        exclusive
        onChange={handleChange}
        disabled={disabled}
        fullWidth
        size="small"
      >
        {tierInfo.map((tier) => (
          <Tooltip key={tier.value} title={tier.description} arrow placement="top">
            <ToggleButton value={tier.value} sx={{ py: 1 }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                {tier.icon}
                <Typography variant="caption" sx={{ mt: 0.5 }}>
                  {tier.tokens}
                </Typography>
              </Box>
            </ToggleButton>
          </Tooltip>
        ))}
      </ToggleButtonGroup>
    </Box>
  );
}
