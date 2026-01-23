import client from './client';
import type {
  WorkQueue,
  WorkQueueDetail,
  WorkQueueItem,
  ContextHelper,
  TieredContextResponse,
  CreateWorkQueueRequest,
  UpdateWorkQueueRequest,
  AddQueueItemRequest,
  UpdateQueueItemRequest,
  ReorderItemsRequest,
  CheckoutRequest,
  UpdateContextRequest,
  WorkQueueStatus,
} from '../types';

export const workQueuesApi = {
  // Queue CRUD
  getByProject: async (projectId: string, status?: WorkQueueStatus): Promise<WorkQueue[]> => {
    const params = status ? { status } : {};
    const response = await client.get<WorkQueue[]>(`/api/projects/${projectId}/queues`, { params });
    return response.data;
  },

  getById: async (projectId: string, queueId: string): Promise<WorkQueueDetail> => {
    const response = await client.get<WorkQueueDetail>(`/api/projects/${projectId}/queues/${queueId}`);
    return response.data;
  },

  create: async (projectId: string, request: CreateWorkQueueRequest): Promise<WorkQueue> => {
    const response = await client.post<WorkQueue>(`/api/projects/${projectId}/queues`, request);
    return response.data;
  },

  update: async (projectId: string, queueId: string, request: UpdateWorkQueueRequest): Promise<WorkQueue> => {
    const response = await client.put<WorkQueue>(`/api/projects/${projectId}/queues/${queueId}`, request);
    return response.data;
  },

  delete: async (projectId: string, queueId: string): Promise<void> => {
    await client.delete(`/api/projects/${projectId}/queues/${queueId}`);
  },

  // Checkout/Release
  checkout: async (projectId: string, queueId: string, request?: CheckoutRequest): Promise<WorkQueueDetail> => {
    const response = await client.post<WorkQueueDetail>(
      `/api/projects/${projectId}/queues/${queueId}/checkout`,
      request || {}
    );
    return response.data;
  },

  release: async (projectId: string, queueId: string): Promise<void> => {
    await client.post(`/api/projects/${projectId}/queues/${queueId}/release`);
  },

  // Queue Items
  getItems: async (projectId: string, queueId: string): Promise<WorkQueueItem[]> => {
    const response = await client.get<WorkQueueItem[]>(`/api/projects/${projectId}/queues/${queueId}/items`);
    return response.data;
  },

  addItem: async (projectId: string, queueId: string, request: AddQueueItemRequest): Promise<WorkQueueItem> => {
    const response = await client.post<WorkQueueItem>(
      `/api/projects/${projectId}/queues/${queueId}/items`,
      request
    );
    return response.data;
  },

  updateItem: async (
    projectId: string,
    queueId: string,
    itemId: string,
    request: UpdateQueueItemRequest
  ): Promise<WorkQueueItem> => {
    const response = await client.patch<WorkQueueItem>(
      `/api/projects/${projectId}/queues/${queueId}/items/${itemId}`,
      request
    );
    return response.data;
  },

  removeItem: async (projectId: string, queueId: string, itemId: string): Promise<void> => {
    await client.delete(`/api/projects/${projectId}/queues/${queueId}/items/${itemId}`);
  },

  reorderItems: async (projectId: string, queueId: string, request: ReorderItemsRequest): Promise<void> => {
    await client.post(`/api/projects/${projectId}/queues/${queueId}/items/reorder`, request);
  },

  // Context
  getContext: async (projectId: string, queueId: string, tier?: number): Promise<ContextHelper | TieredContextResponse> => {
    const params = tier ? { tier } : {};
    const response = await client.get<ContextHelper | TieredContextResponse>(
      `/api/projects/${projectId}/queues/${queueId}/context`,
      { params }
    );
    return response.data;
  },

  getTieredContext: async (projectId: string, queueId: string, tier: number): Promise<TieredContextResponse> => {
    const response = await client.get<TieredContextResponse>(
      `/api/projects/${projectId}/queues/${queueId}/context`,
      { params: { tier } }
    );
    return response.data;
  },

  updateContext: async (projectId: string, queueId: string, request: UpdateContextRequest): Promise<ContextHelper> => {
    const response = await client.patch<ContextHelper>(
      `/api/projects/${projectId}/queues/${queueId}/context`,
      request
    );
    return response.data;
  },
};
