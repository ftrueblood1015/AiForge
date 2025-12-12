import { Box, CircularProgress, Typography, Skeleton, Card, CardContent, Grid } from '@mui/material';

interface LoadingSpinnerProps {
  message?: string;
  size?: number;
}

export function LoadingSpinner({ message, size = 40 }: LoadingSpinnerProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: 200,
        gap: 2,
      }}
    >
      <CircularProgress size={size} />
      {message && (
        <Typography variant="body2" color="text.secondary">
          {message}
        </Typography>
      )}
    </Box>
  );
}

interface CardSkeletonProps {
  count?: number;
  height?: number;
}

export function CardSkeleton({ count = 3, height = 150 }: CardSkeletonProps) {
  return (
    <Grid container spacing={2}>
      {Array.from({ length: count }).map((_, index) => (
        <Grid key={index} size={{ xs: 12, md: 6, lg: 4 }}>
          <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 2 }} />
        </Grid>
      ))}
    </Grid>
  );
}

interface ListSkeletonProps {
  count?: number;
  height?: number;
}

export function ListSkeleton({ count = 5, height = 72 }: ListSkeletonProps) {
  return (
    <Box>
      {Array.from({ length: count }).map((_, index) => (
        <Skeleton
          key={index}
          variant="rectangular"
          height={height}
          sx={{ mb: 1, borderRadius: 1 }}
        />
      ))}
    </Box>
  );
}

interface DetailSkeletonProps {
  sections?: number;
}

export function DetailSkeleton({ sections = 3 }: DetailSkeletonProps) {
  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Skeleton variant="circular" width={48} height={48} />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width={200} height={32} />
          <Skeleton variant="text" width={150} height={20} />
        </Box>
      </Box>

      {/* Content sections */}
      {Array.from({ length: sections }).map((_, index) => (
        <Card key={index} sx={{ mb: 2 }}>
          <CardContent>
            <Skeleton variant="text" width={120} height={24} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={80} sx={{ borderRadius: 1 }} />
          </CardContent>
        </Card>
      ))}
    </Box>
  );
}

interface TableSkeletonProps {
  rows?: number;
  columns?: number;
}

export function TableSkeleton({ rows = 5, columns = 4 }: TableSkeletonProps) {
  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', gap: 2, mb: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
        {Array.from({ length: columns }).map((_, index) => (
          <Skeleton key={index} variant="text" width={`${100 / columns}%`} height={24} />
        ))}
      </Box>
      {/* Rows */}
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <Box
          key={rowIndex}
          sx={{
            display: 'flex',
            gap: 2,
            p: 2,
            borderBottom: 1,
            borderColor: 'divider',
          }}
        >
          {Array.from({ length: columns }).map((_, colIndex) => (
            <Skeleton
              key={colIndex}
              variant="text"
              width={`${100 / columns}%`}
              height={20}
            />
          ))}
        </Box>
      ))}
    </Box>
  );
}
