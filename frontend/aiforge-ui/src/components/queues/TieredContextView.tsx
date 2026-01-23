import { useState, useEffect } from 'react';
import {
  Card,
  CardHeader,
  CardContent,
  Box,
  Skeleton,
  Alert,
  IconButton,
  Tooltip,
} from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';
import TierSelector from './TierSelector';
import TierContent from './TierContent';
import type { TieredContextResponse } from '../../types';

interface TieredContextViewProps {
  projectId: string;
  queueId: string;
  initialTier?: number;
  onFetchTier: (tier: number) => Promise<TieredContextResponse>;
}

export default function TieredContextView({
  projectId,
  queueId,
  initialTier = 1,
  onFetchTier,
}: TieredContextViewProps) {
  const [selectedTier, setSelectedTier] = useState(initialTier);
  const [tieredData, setTieredData] = useState<TieredContextResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTierData = async (tier: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await onFetchTier(tier);
      setTieredData(data);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTierData(selectedTier);
  }, [selectedTier, projectId, queueId]);

  const handleTierChange = (tier: number) => {
    setSelectedTier(tier);
  };

  const handleRefresh = () => {
    fetchTierData(selectedTier);
  };

  return (
    <Card>
      <CardHeader
        title="Tiered Context"
        subheader="Progressive detail levels for AI context"
        action={
          <Tooltip title="Refresh">
            <IconButton onClick={handleRefresh} disabled={isLoading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        }
      />
      <CardContent>
        {/* Tier Selector */}
        <Box sx={{ mb: 3 }}>
          <TierSelector
            value={selectedTier}
            onChange={handleTierChange}
            disabled={isLoading}
            estimatedTokens={tieredData?.estimatedTokens}
          />
        </Box>

        {/* Error State */}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {/* Loading State */}
        {isLoading && !tieredData && (
          <Box>
            <Skeleton variant="rectangular" height={200} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={100} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={100} />
          </Box>
        )}

        {/* Content */}
        {tieredData && (
          <Box sx={{ opacity: isLoading ? 0.6 : 1, transition: 'opacity 0.2s' }}>
            <TierContent data={tieredData} selectedTier={selectedTier} />
          </Box>
        )}
      </CardContent>
    </Card>
  );
}
