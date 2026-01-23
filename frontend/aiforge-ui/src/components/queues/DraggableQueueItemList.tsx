import { useCallback } from 'react';
import {
  DragDropContext,
  Droppable,
  Draggable,
  type DropResult,
} from '@hello-pangea/dnd';
import {
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Box,
  Typography,
  Chip,
  Paper,
} from '@mui/material';
import {
  DragIndicator as DragIcon,
  Assignment as TaskIcon,
  AutoStories as StoryIcon,
} from '@mui/icons-material';
import QueueItemStatusChip from './QueueItemStatusChip';
import type { WorkQueueItem, WorkItemType, WorkQueueItemStatus } from '../../types';

const itemTypeIcons: Record<WorkItemType, React.ReactNode> = {
  Task: <TaskIcon color="action" fontSize="small" />,
  UserStory: <StoryIcon color="primary" fontSize="small" />,
};

interface DraggableQueueItemListProps {
  items: WorkQueueItem[];
  onReorder: (itemIds: string[]) => Promise<void>;
  onStatusChange?: (itemId: string, status: WorkQueueItemStatus) => void;
  disabled?: boolean;
}

export default function DraggableQueueItemList({
  items,
  onReorder,
  onStatusChange,
  disabled = false,
}: DraggableQueueItemListProps) {
  const sortedItems = [...items].sort((a, b) => a.position - b.position);

  const handleDragEnd = useCallback(
    async (result: DropResult) => {
      if (!result.destination) return;
      if (result.source.index === result.destination.index) return;

      // Reorder items
      const reordered = Array.from(sortedItems);
      const [removed] = reordered.splice(result.source.index, 1);
      reordered.splice(result.destination.index, 0, removed);

      // Get new order of IDs
      const newOrder = reordered.map((item) => item.id);
      await onReorder(newOrder);
    },
    [sortedItems, onReorder]
  );

  const getNextStatus = (current: WorkQueueItemStatus): WorkQueueItemStatus => {
    const statusOrder: WorkQueueItemStatus[] = ['Pending', 'InProgress', 'Completed'];
    const currentIndex = statusOrder.indexOf(current);
    if (currentIndex === -1 || currentIndex === statusOrder.length - 1) {
      return 'Pending';
    }
    return statusOrder[currentIndex + 1];
  };

  if (sortedItems.length === 0) {
    return (
      <Box sx={{ py: 4, textAlign: 'center' }}>
        <Typography color="text.secondary">
          No items in this queue yet.
        </Typography>
      </Box>
    );
  }

  return (
    <DragDropContext onDragEnd={handleDragEnd}>
      <Droppable droppableId="queue-items" isDropDisabled={disabled}>
        {(provided, snapshot) => (
          <List
            disablePadding
            ref={provided.innerRef}
            {...provided.droppableProps}
            sx={{
              backgroundColor: snapshot.isDraggingOver ? 'action.hover' : 'transparent',
              borderRadius: 1,
              transition: 'background-color 0.2s ease',
            }}
          >
            {sortedItems.map((item, index) => (
              <Draggable
                key={item.id}
                draggableId={item.id}
                index={index}
                isDragDisabled={disabled}
              >
                {(provided, snapshot) => (
                  <Paper
                    ref={provided.innerRef}
                    {...provided.draggableProps}
                    elevation={snapshot.isDragging ? 4 : 0}
                    sx={{
                      mb: 1,
                      backgroundColor: snapshot.isDragging
                        ? 'background.paper'
                        : 'transparent',
                      border: '1px solid',
                      borderColor: snapshot.isDragging ? 'primary.main' : 'divider',
                      borderRadius: 1,
                      transition: 'all 0.2s ease',
                    }}
                  >
                    <ListItem
                      sx={{
                        '&:hover': { backgroundColor: 'action.hover' },
                      }}
                    >
                      {/* Drag Handle */}
                      <ListItemIcon
                        {...provided.dragHandleProps}
                        sx={{
                          minWidth: 32,
                          cursor: disabled ? 'default' : 'grab',
                          color: 'text.disabled',
                          '&:hover': { color: 'text.primary' },
                        }}
                      >
                        <DragIcon />
                      </ListItemIcon>

                      {/* Item Type Icon */}
                      <ListItemIcon sx={{ minWidth: 36 }}>
                        {itemTypeIcons[item.workItemType]}
                      </ListItemIcon>

                      {/* Item Content */}
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Chip
                              label={`#${item.position}`}
                              size="small"
                              variant="outlined"
                              sx={{ minWidth: 40 }}
                            />
                            <Typography variant="body1">
                              {item.workItemTitle || 'Untitled Item'}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          item.notes && (
                            <Typography variant="caption" color="text.secondary">
                              {item.notes}
                            </Typography>
                          )
                        }
                      />

                      {/* Status Chip */}
                      <QueueItemStatusChip
                        status={item.status}
                        onClick={
                          onStatusChange
                            ? () => onStatusChange(item.id, getNextStatus(item.status))
                            : undefined
                        }
                      />
                    </ListItem>
                  </Paper>
                )}
              </Draggable>
            ))}
            {provided.placeholder}
          </List>
        )}
      </Droppable>
    </DragDropContext>
  );
}
