import { Box, LinearProgress, Typography } from '@mui/material';

interface SubTicketProgressProps {
  total: number;
  completed: number;
  progress: number;
}

export default function SubTicketProgress({ total, completed, progress }: SubTicketProgressProps) {
  if (total === 0) return null;

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <LinearProgress
        variant="determinate"
        value={progress}
        sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
      />
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 40 }}>
        {completed}/{total}
      </Typography>
    </Box>
  );
}
