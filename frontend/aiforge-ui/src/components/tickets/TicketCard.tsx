import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Chip,
  Box,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Assignment as TaskIcon,
  BugReport as BugIcon,
  Star as FeatureIcon,
  Build as EnhancementIcon,
  MoreVert as MoreIcon,
} from '@mui/icons-material';
import type { Ticket, TicketType, Priority } from '../../types';

const typeIcons: Record<TicketType, React.ReactNode> = {
  Task: <TaskIcon fontSize="small" color="action" />,
  Bug: <BugIcon fontSize="small" color="error" />,
  Feature: <FeatureIcon fontSize="small" color="warning" />,
  Enhancement: <EnhancementIcon fontSize="small" color="info" />,
};

const priorityColors: Record<Priority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

interface TicketCardProps {
  ticket: Ticket;
  onClick?: (ticket: Ticket) => void;
  onMenuClick?: (event: React.MouseEvent, ticket: Ticket) => void;
  isDragging?: boolean;
  compact?: boolean;
}

export default function TicketCard({
  ticket,
  onClick,
  onMenuClick,
  isDragging = false,
  compact = false,
}: TicketCardProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(ticket);
    }
  };

  const handleMenuClick = (event: React.MouseEvent) => {
    event.stopPropagation();
    if (onMenuClick) {
      onMenuClick(event, ticket);
    }
  };

  return (
    <Card
      sx={{
        cursor: onClick ? 'pointer' : 'default',
        transition: 'box-shadow 0.2s, transform 0.2s',
        transform: isDragging ? 'rotate(3deg)' : 'none',
        boxShadow: isDragging ? 4 : 1,
        '&:hover': onClick
          ? {
              boxShadow: 3,
              transform: 'translateY(-2px)',
            }
          : {},
        opacity: isDragging ? 0.9 : 1,
      }}
      onClick={handleClick}
    >
      <CardContent sx={{ p: compact ? 1.5 : 2, '&:last-child': { pb: compact ? 1.5 : 2 } }}>
        {/* Header with type icon and menu */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            {typeIcons[ticket.type]}
            <Typography variant="caption" color="text.secondary" fontWeight={500}>
              {ticket.key}
            </Typography>
          </Box>
          {onMenuClick && (
            <Tooltip title="More options">
              <IconButton size="small" onClick={handleMenuClick} sx={{ mt: -0.5, mr: -0.5 }}>
                <MoreIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>

        {/* Title */}
        <Typography
          variant="body2"
          fontWeight={500}
          sx={{
            mb: 1,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: compact ? 1 : 2,
            WebkitBoxOrient: 'vertical',
          }}
        >
          {ticket.title}
        </Typography>

        {/* Footer with priority */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Chip
            label={ticket.priority}
            size="small"
            color={priorityColors[ticket.priority]}
            sx={{ height: 20, fontSize: '0.7rem' }}
          />
          {!compact && ticket.description && (
            <Typography variant="caption" color="text.secondary">
              Has description
            </Typography>
          )}
        </Box>
      </CardContent>
    </Card>
  );
}
