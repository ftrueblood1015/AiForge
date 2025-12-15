import client from './client';
import type {
  EffortEstimation,
  EstimationHistoryResponse,
  CreateEstimationRequest,
  ReviseEstimationRequest,
  RecordActualEffortRequest,
} from '../types';

export const estimationApi = {
  getLatest: async (ticketId: string): Promise<EffortEstimation | null> => {
    try {
      const response = await client.get<EffortEstimation>(`/api/tickets/${ticketId}/estimation`);
      return response.data;
    } catch (error: unknown) {
      // Return null if no estimation exists (404)
      if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as { response?: { status: number } };
        if (axiosError.response?.status === 404) {
          return null;
        }
      }
      throw error;
    }
  },

  getHistory: async (ticketId: string): Promise<EstimationHistoryResponse> => {
    const response = await client.get<EstimationHistoryResponse>(
      `/api/tickets/${ticketId}/estimation/history`
    );
    return response.data;
  },

  create: async (ticketId: string, request: CreateEstimationRequest): Promise<EffortEstimation> => {
    const response = await client.post<EffortEstimation>(
      `/api/tickets/${ticketId}/estimation`,
      request
    );
    return response.data;
  },

  revise: async (ticketId: string, request: ReviseEstimationRequest): Promise<EffortEstimation> => {
    const response = await client.put<EffortEstimation>(
      `/api/tickets/${ticketId}/estimation`,
      request
    );
    return response.data;
  },

  recordActual: async (
    ticketId: string,
    request: RecordActualEffortRequest
  ): Promise<EffortEstimation> => {
    const response = await client.post<EffortEstimation>(
      `/api/tickets/${ticketId}/estimation/actual`,
      request
    );
    return response.data;
  },
};
