import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Skeleton,
} from '@mui/material';
import {
  DragDropContext,
  Droppable,
  Draggable,
  type DropResult,
} from '@hello-pangea/dnd';
import TicketCard from './TicketCard';
import type { Ticket, TicketStatus } from '../../types';

const STATUSES: TicketStatus[] = ['ToDo', 'InProgress', 'InReview', 'Done'];

const statusLabels: Record<TicketStatus, string> = {
  ToDo: 'To Do',
  InProgress: 'In Progress',
  InReview: 'In Review',
  Done: 'Done',
};

const statusColors: Record<TicketStatus, string> = {
  ToDo: '#9e9e9e',
  InProgress: '#1976d2',
  InReview: '#ed6c02',
  Done: '#2e7d32',
};

interface TicketBoardProps {
  tickets: Ticket[];
  isLoading?: boolean;
  onTicketClick?: (ticket: Ticket) => void;
  onStatusChange?: (ticketId: string, newStatus: TicketStatus) => Promise<void>;
}

export default function TicketBoard({
  tickets,
  isLoading = false,
  onTicketClick,
  onStatusChange,
}: TicketBoardProps) {
  // Group tickets by status
  const ticketsByStatus = STATUSES.reduce(
    (acc, status) => {
      acc[status] = tickets.filter((t) => t.status === status);
      return acc;
    },
    {} as Record<TicketStatus, Ticket[]>
  );

  const handleDragEnd = async (result: DropResult) => {
    const { destination, source, draggableId } = result;

    // Dropped outside a droppable area
    if (!destination) return;

    // Dropped in the same position
    if (
      destination.droppableId === source.droppableId &&
      destination.index === source.index
    ) {
      return;
    }

    const newStatus = destination.droppableId as TicketStatus;

    // If status changed, call the handler
    if (source.droppableId !== destination.droppableId && onStatusChange) {
      await onStatusChange(draggableId, newStatus);
    }
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', gap: 2, overflowX: 'auto', pb: 2 }}>
        {STATUSES.map((status) => (
          <Box key={status} sx={{ minWidth: 280, flex: '0 0 280px' }}>
            <Card sx={{ backgroundColor: 'background.default' }}>
              <CardContent>
                <Skeleton variant="text" width={100} height={32} sx={{ mb: 2 }} />
                {[1, 2, 3].map((i) => (
                  <Skeleton
                    key={i}
                    variant="rectangular"
                    height={100}
                    sx={{ mb: 1, borderRadius: 1 }}
                  />
                ))}
              </CardContent>
            </Card>
          </Box>
        ))}
      </Box>
    );
  }

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          overflowX: 'auto',
          pb: 2,
          minHeight: 400,
        }}
      >
        {STATUSES.map((status) => (
          <Box key={status} sx={{ minWidth: 280, flex: '0 0 280px' }}>
            <Card
              sx={{
                backgroundColor: 'background.default',
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
              }}
            >
              <CardContent sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                {/* Column Header */}
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    mb: 2,
                    pb: 1,
                    borderBottom: 2,
                    borderColor: statusColors[status],
                  }}
                >
                  <Typography variant="subtitle1" fontWeight={600}>
                    {statusLabels[status]}
                  </Typography>
                  <Chip
                    label={ticketsByStatus[status].length}
                    size="small"
                    sx={{
                      backgroundColor: statusColors[status],
                      color: 'white',
                      fontWeight: 600,
                    }}
                  />
                </Box>

                {/* Droppable Area */}
                <Droppable droppableId={status}>
                  {(provided, snapshot) => (
                    <Box
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      sx={{
                        flex: 1,
                        minHeight: 100,
                        borderRadius: 1,
                        backgroundColor: snapshot.isDraggingOver
                          ? 'action.hover'
                          : 'transparent',
                        transition: 'background-color 0.2s',
                        p: snapshot.isDraggingOver ? 1 : 0,
                      }}
                    >
                      {ticketsByStatus[status].length === 0 ? (
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            height: 100,
                            color: 'text.secondary',
                            border: '2px dashed',
                            borderColor: 'divider',
                            borderRadius: 1,
                          }}
                        >
                          <Typography variant="body2">No tickets</Typography>
                        </Box>
                      ) : (
                        ticketsByStatus[status].map((ticket, index) => (
                          <Draggable
                            key={ticket.id}
                            draggableId={ticket.id}
                            index={index}
                          >
                            {(provided, snapshot) => (
                              <Box
                                ref={provided.innerRef}
                                {...provided.draggableProps}
                                {...provided.dragHandleProps}
                                sx={{ mb: 1 }}
                              >
                                <TicketCard
                                  ticket={ticket}
                                  onClick={onTicketClick}
                                  isDragging={snapshot.isDragging}
                                  compact
                                />
                              </Box>
                            )}
                          </Draggable>
                        ))
                      )}
                      {provided.placeholder}
                    </Box>
                  )}
                </Droppable>
              </CardContent>
            </Card>
          </Box>
        ))}
      </Box>
    </DragDropContext>
  );
}
