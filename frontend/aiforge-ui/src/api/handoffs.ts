import client from './client';
import type { HandoffDocument, FileSnapshot } from '../types';

export interface HandoffSearchParams {
  ticketId?: string;
  type?: string;
  search?: string;
}

export const handoffsApi = {
  // List handoffs with optional filters
  search: async (params?: HandoffSearchParams): Promise<HandoffDocument[]> => {
    const response = await client.get<HandoffDocument[]>('/api/handoffs', { params });
    return response.data;
  },

  // Get handoff by ID
  getById: async (id: string): Promise<HandoffDocument> => {
    const response = await client.get<HandoffDocument>(`/api/handoffs/${id}`);
    return response.data;
  },

  // Get latest active handoff for a ticket
  getLatestForTicket: async (ticketId: string): Promise<HandoffDocument | null> => {
    try {
      const response = await client.get<HandoffDocument>(`/api/handoffs/ticket/${ticketId}/latest`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  // Get handoffs for a ticket
  getByTicket: async (ticketId: string): Promise<HandoffDocument[]> => {
    const response = await client.get<HandoffDocument[]>('/api/handoffs', {
      params: { ticketId },
    });
    return response.data;
  },

  // Create handoff
  create: async (
    ticketId: string,
    title: string,
    type: 'SessionEnd' | 'Blocker' | 'Milestone' | 'ContextDump',
    summary: string,
    content: string,
    structuredContext?: object
  ): Promise<HandoffDocument> => {
    const response = await client.post<HandoffDocument>('/api/handoffs', {
      ticketId,
      title,
      type,
      summary,
      content,
      structuredContext,
    });
    return response.data;
  },

  // Get file snapshots for a handoff
  getSnapshots: async (handoffId: string): Promise<FileSnapshot[]> => {
    const response = await client.get<FileSnapshot[]>(`/api/handoffs/${handoffId}/snapshots`);
    return response.data;
  },

  // Add file snapshot
  addSnapshot: async (
    handoffId: string,
    filePath: string,
    contentBefore: string | null,
    contentAfter: string | null,
    language: string
  ): Promise<FileSnapshot> => {
    const response = await client.post<FileSnapshot>(`/api/handoffs/${handoffId}/snapshots`, {
      filePath,
      contentBefore,
      contentAfter,
      language,
    });
    return response.data;
  },
};
