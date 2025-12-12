import { Box, Typography, Card, CardContent } from '@mui/material';
import { Description as DescriptionIcon } from '@mui/icons-material';

export default function HandoffList() {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Handoffs
      </Typography>

      <Card>
        <CardContent sx={{ textAlign: 'center', py: 6 }}>
          <DescriptionIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Handoffs Coming Soon
          </Typography>
          <Typography variant="body2" color="text.secondary">
            This page will display AI session handoff documents
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
