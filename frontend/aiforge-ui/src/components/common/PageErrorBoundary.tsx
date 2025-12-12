import { Box, Typography, Button, Container } from '@mui/material';
import { ErrorOutline as ErrorIcon, Home as HomeIcon } from '@mui/icons-material';
import { useNavigate, useRouteError, isRouteErrorResponse } from 'react-router-dom';

export default function PageErrorBoundary() {
  const error = useRouteError();
  const navigate = useNavigate();

  let title = 'Something went wrong';
  let message = 'An unexpected error occurred';

  if (isRouteErrorResponse(error)) {
    if (error.status === 404) {
      title = 'Page not found';
      message = "The page you're looking for doesn't exist";
    } else if (error.status === 403) {
      title = 'Access denied';
      message = "You don't have permission to view this page";
    } else if (error.status === 500) {
      title = 'Server error';
      message = 'Something went wrong on our end';
    }
  } else if (error instanceof Error) {
    message = error.message;
  }

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '60vh',
          textAlign: 'center',
        }}
      >
        <ErrorIcon sx={{ fontSize: 80, color: 'error.main', mb: 3 }} />
        <Typography variant="h4" gutterBottom>
          {title}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          {message}
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button variant="outlined" onClick={() => navigate(-1)}>
            Go Back
          </Button>
          <Button
            variant="contained"
            startIcon={<HomeIcon />}
            onClick={() => navigate('/')}
          >
            Go Home
          </Button>
        </Box>
      </Box>
    </Container>
  );
}
