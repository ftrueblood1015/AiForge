import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Breadcrumbs,
  Link,
  Skeleton,
  Alert,
  Button,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { handoffsApi } from '../api/handoffs';
import { HandoffViewer } from '../components/handoffs';
import type { HandoffDocument, FileSnapshot } from '../types';

export default function HandoffDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [handoff, setHandoff] = useState<HandoffDocument | null>(null);
  const [snapshots, setSnapshots] = useState<FileSnapshot[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadHandoff(id);
    }
  }, [id]);

  const loadHandoff = async (handoffId: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const [handoffData, snapshotData] = await Promise.all([
        handoffsApi.getById(handoffId),
        handoffsApi.getSnapshots(handoffId),
      ]);
      setHandoff(handoffData);
      setSnapshots(snapshotData);
    } catch (err) {
      setError('Failed to load handoff');
      console.error('Error loading handoff:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (error) {
    return (
      <Box>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/handoffs')}
          sx={{ mb: 2 }}
        >
          Back to Handoffs
        </Button>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (isLoading || !handoff) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={40} />
        <Skeleton variant="rectangular" height={200} sx={{ mt: 2, borderRadius: 2 }} />
        <Skeleton variant="rectangular" height={400} sx={{ mt: 2, borderRadius: 2 }} />
      </Box>
    );
  }

  return (
    <Box>
      {/* Breadcrumbs */}
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          component="button"
          variant="body2"
          onClick={() => navigate('/handoffs')}
          underline="hover"
          color="inherit"
        >
          Handoffs
        </Link>
        <Typography color="text.primary">{handoff.title}</Typography>
      </Breadcrumbs>

      {/* Handoff Viewer */}
      <HandoffViewer handoff={handoff} snapshots={snapshots} />
    </Box>
  );
}
