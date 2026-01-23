import {
  ListItem,
  ListItemText,
  ListItemIcon,
  Box,
  Typography,
  Chip,
  Tooltip,
} from '@mui/material';
import {
  Queue as QueueIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import QueueStatusChip from './QueueStatusChip';
import type { WorkQueue } from '../../types';

interface QueueListItemProps {
  queue: WorkQueue;
  onClick: () => void;
}

export default function QueueListItem({ queue, onClick }: QueueListItemProps) {
  const isCheckedOut = !!queue.checkedOutBy;

  return (
    <ListItem
      sx={{
        borderRadius: 1,
        mb: 1,
        cursor: 'pointer',
        '&:hover': { backgroundColor: 'action.hover' },
        border: '1px solid',
        borderColor: 'divider',
      }}
      onClick={onClick}
    >
      <ListItemIcon>
        {isCheckedOut ? (
          <Tooltip title={`Checked out by ${queue.checkedOutBy}`}>
            <LockIcon color="warning" />
          </Tooltip>
        ) : (
          <QueueIcon color="primary" />
        )}
      </ListItemIcon>
      <ListItemText
        primary={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="body1" fontWeight={500}>
              {queue.name}
            </Typography>
            {isCheckedOut && (
              <Chip
                label={`Locked by ${queue.checkedOutBy}`}
                size="small"
                variant="outlined"
                color="warning"
                icon={<LockIcon />}
              />
            )}
          </Box>
        }
        secondary={
          <Typography variant="caption" color="text.secondary">
            {queue.description || 'No description'} • {queue.itemCount} items
          </Typography>
        }
      />
      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
        <Chip
          label={`${queue.itemCount} items`}
          size="small"
          variant="outlined"
        />
        <QueueStatusChip status={queue.status} />
      </Box>
    </ListItem>
  );
}
