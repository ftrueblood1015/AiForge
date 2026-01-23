import { create } from 'zustand';
import type {
  WorkQueue,
  WorkQueueDetail,
  WorkQueueItem,
  ContextHelper,
  TieredContextResponse,
  WorkQueueStatus,
  WorkQueueItemStatus,
  CreateWorkQueueRequest,
  UpdateContextRequest,
} from '../types';
import { workQueuesApi } from '../api/workQueues';

interface QueueState {
  // Data
  queues: WorkQueue[];
  currentQueue: WorkQueueDetail | null;
  tieredContext: TieredContextResponse | null;

  // UI State
  isLoading: boolean;
  error: string | null;
  selectedQueueId: string | null;
  isEditingContext: boolean;
  currentTier: number;
  statusFilter: WorkQueueStatus | null;

  // Queue Actions
  fetchQueues: (projectId: string, status?: WorkQueueStatus) => Promise<void>;
  fetchQueue: (projectId: string, queueId: string) => Promise<void>;
  createQueue: (projectId: string, request: CreateWorkQueueRequest) => Promise<WorkQueue>;
  deleteQueue: (projectId: string, queueId: string) => Promise<void>;

  // Checkout Actions
  checkoutQueue: (projectId: string, queueId: string, durationMinutes?: number) => Promise<void>;
  releaseQueue: (projectId: string, queueId: string) => Promise<void>;

  // Item Actions
  updateItemStatus: (projectId: string, queueId: string, itemId: string, status: WorkQueueItemStatus) => Promise<void>;
  reorderItems: (projectId: string, queueId: string, itemIds: string[]) => Promise<void>;

  // Context Actions
  fetchTieredContext: (projectId: string, queueId: string, tier: number) => Promise<void>;
  updateContext: (projectId: string, queueId: string, request: UpdateContextRequest) => Promise<void>;

  // UI Actions
  selectQueue: (id: string | null) => void;
  setEditingContext: (editing: boolean) => void;
  setTier: (tier: number) => void;
  setStatusFilter: (status: WorkQueueStatus | null) => void;
  clearError: () => void;
}

export const useQueueStore = create<QueueState>((set, get) => ({
  // Initial State
  queues: [],
  currentQueue: null,
  tieredContext: null,
  isLoading: false,
  error: null,
  selectedQueueId: null,
  isEditingContext: false,
  currentTier: 1,
  statusFilter: null,

  // Queue Actions
  fetchQueues: async (projectId: string, status?: WorkQueueStatus) => {
    set({ isLoading: true, error: null });
    try {
      const queues = await workQueuesApi.getByProject(projectId, status);
      set({ queues, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchQueue: async (projectId: string, queueId: string) => {
    set({ isLoading: true, error: null });
    try {
      const queue = await workQueuesApi.getById(projectId, queueId);
      set({ currentQueue: queue, selectedQueueId: queueId, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createQueue: async (projectId: string, request: CreateWorkQueueRequest) => {
    set({ isLoading: true, error: null });
    try {
      const queue = await workQueuesApi.create(projectId, request);
      set((state) => ({
        queues: [...state.queues, queue],
        isLoading: false,
      }));
      return queue;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  deleteQueue: async (projectId: string, queueId: string) => {
    set({ isLoading: true, error: null });
    try {
      await workQueuesApi.delete(projectId, queueId);
      set((state) => ({
        queues: state.queues.filter((q) => q.id !== queueId),
        currentQueue: state.currentQueue?.id === queueId ? null : state.currentQueue,
        selectedQueueId: state.selectedQueueId === queueId ? null : state.selectedQueueId,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  // Checkout Actions
  checkoutQueue: async (projectId: string, queueId: string, durationMinutes?: number) => {
    set({ isLoading: true, error: null });
    try {
      const queue = await workQueuesApi.checkout(projectId, queueId, { durationMinutes });
      set((state) => ({
        currentQueue: queue,
        queues: state.queues.map((q) =>
          q.id === queueId
            ? { ...q, checkedOutBy: queue.checkedOutBy, checkedOutAt: queue.checkedOutAt, checkoutExpiresAt: queue.checkoutExpiresAt }
            : q
        ),
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  releaseQueue: async (projectId: string, queueId: string) => {
    set({ isLoading: true, error: null });
    try {
      await workQueuesApi.release(projectId, queueId);
      set((state) => ({
        currentQueue: state.currentQueue
          ? { ...state.currentQueue, checkedOutBy: null, checkedOutAt: null, checkoutExpiresAt: null }
          : null,
        queues: state.queues.map((q) =>
          q.id === queueId
            ? { ...q, checkedOutBy: null, checkedOutAt: null, checkoutExpiresAt: null }
            : q
        ),
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  // Item Actions
  updateItemStatus: async (projectId: string, queueId: string, itemId: string, status: WorkQueueItemStatus) => {
    try {
      const updatedItem = await workQueuesApi.updateItem(projectId, queueId, itemId, { status });
      set((state) => ({
        currentQueue: state.currentQueue
          ? {
              ...state.currentQueue,
              items: state.currentQueue.items.map((item) =>
                item.id === itemId ? updatedItem : item
              ),
            }
          : null,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
      throw error;
    }
  },

  reorderItems: async (projectId: string, queueId: string, itemIds: string[]) => {
    // Optimistic update
    const previousItems = get().currentQueue?.items || [];

    set((state) => {
      if (!state.currentQueue) return state;

      const reorderedItems = itemIds
        .map((id, index) => {
          const item = state.currentQueue!.items.find((i) => i.id === id);
          return item ? { ...item, position: index + 1 } : null;
        })
        .filter((item): item is WorkQueueItem => item !== null);

      return {
        currentQueue: {
          ...state.currentQueue,
          items: reorderedItems,
        },
      };
    });

    try {
      await workQueuesApi.reorderItems(projectId, queueId, { itemIds });
    } catch (error) {
      // Rollback on error
      set((state) => ({
        currentQueue: state.currentQueue
          ? { ...state.currentQueue, items: previousItems }
          : null,
        error: (error as Error).message,
      }));
      throw error;
    }
  },

  // Context Actions
  fetchTieredContext: async (projectId: string, queueId: string, tier: number) => {
    set({ isLoading: true, error: null, currentTier: tier });
    try {
      const context = await workQueuesApi.getTieredContext(projectId, queueId, tier);
      set({ tieredContext: context, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  updateContext: async (projectId: string, queueId: string, request: UpdateContextRequest) => {
    set({ isLoading: true, error: null });
    try {
      const context = await workQueuesApi.updateContext(projectId, queueId, request);
      set((state) => ({
        currentQueue: state.currentQueue
          ? { ...state.currentQueue, context }
          : null,
        tieredContext: state.tieredContext
          ? { ...state.tieredContext, tier1: { ...state.tieredContext.tier1, context } }
          : null,
        isLoading: false,
        isEditingContext: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  // UI Actions
  selectQueue: (id: string | null) => set({ selectedQueueId: id }),
  setEditingContext: (editing: boolean) => set({ isEditingContext: editing }),
  setTier: (tier: number) => set({ currentTier: tier }),
  setStatusFilter: (status: WorkQueueStatus | null) => set({ statusFilter: status }),
  clearError: () => set({ error: null }),
}));
