import { Box, IconButton, Link, ListItem, ListItemText, Typography } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import type { SubTicketSummary } from '../../types';
import StatusChip from './StatusChip';

interface SubTicketItemProps {
  subTicket: SubTicketSummary;
  onDelete?: (id: string) => void;
  onClick?: (subTicket: SubTicketSummary) => void;
}

export default function SubTicketItem({ subTicket, onDelete, onClick }: SubTicketItemProps) {
  return (
    <ListItem
      sx={{
        borderLeft: 2,
        borderColor: 'primary.main',
        pl: 2,
        '&:hover': { bgcolor: 'action.hover' }
      }}
      secondaryAction={
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <StatusChip status={subTicket.status} size="small" />
          {onDelete && (
            <IconButton size="small" onClick={() => onDelete(subTicket.id)}>
              <DeleteIcon fontSize="small" />
            </IconButton>
          )}
        </Box>
      }
    >
      <ListItemText
        primary={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ fontFamily: 'monospace' }}
            >
              {subTicket.key}
            </Typography>
            <Link
              component="button"
              variant="body2"
              onClick={() => onClick?.(subTicket)}
              sx={{ textAlign: 'left' }}
            >
              {subTicket.title}
            </Link>
          </Box>
        }
      />
    </ListItem>
  );
}
