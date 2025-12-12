import client from './client';
import type { Ticket, CreateTicketRequest, UpdateTicketRequest, TransitionTicketRequest, TicketSearchParams, Comment, TicketHistory } from '../types';

export const ticketsApi = {
  search: async (params: TicketSearchParams): Promise<Ticket[]> => {
    const response = await client.get<Ticket[]>('/api/tickets', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Ticket> => {
    const response = await client.get<Ticket>(`/api/tickets/${id}`);
    return response.data;
  },

  getByKey: async (key: string): Promise<Ticket> => {
    const response = await client.get<Ticket>(`/api/tickets/key/${key}`);
    return response.data;
  },

  create: async (request: CreateTicketRequest): Promise<Ticket> => {
    const response = await client.post<Ticket>('/api/tickets', request);
    return response.data;
  },

  update: async (id: string, request: UpdateTicketRequest): Promise<Ticket> => {
    const response = await client.put<Ticket>(`/api/tickets/${id}`, request);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/tickets/${id}`);
  },

  transition: async (id: string, request: TransitionTicketRequest): Promise<Ticket> => {
    const response = await client.post<Ticket>(`/api/tickets/${id}/transition`, request);
    return response.data;
  },

  // Comments
  getComments: async (ticketId: string): Promise<Comment[]> => {
    const response = await client.get<Comment[]>(`/api/tickets/${ticketId}/comments`);
    return response.data;
  },

  addComment: async (ticketId: string, content: string, isAiGenerated: boolean = false): Promise<Comment> => {
    const response = await client.post<Comment>(`/api/tickets/${ticketId}/comments`, {
      content,
      isAiGenerated,
    });
    return response.data;
  },

  // History
  getHistory: async (ticketId: string): Promise<TicketHistory[]> => {
    const response = await client.get<TicketHistory[]>(`/api/tickets/${ticketId}/history`);
    return response.data;
  },
};
