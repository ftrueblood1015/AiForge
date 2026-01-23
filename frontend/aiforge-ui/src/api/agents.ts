import client from './client';
import type {
  Agent,
  AgentListResponse,
  CreateAgentRequest,
  UpdateAgentRequest,
  AgentSearchParams,
} from '../types';

export const agentsApi = {
  getAll: async (params?: AgentSearchParams): Promise<AgentListResponse> => {
    const response = await client.get<AgentListResponse>('/api/agents', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Agent> => {
    const response = await client.get<Agent>(`/api/agents/${id}`);
    return response.data;
  },

  getByKey: async (
    key: string,
    organizationId: string,
    projectId?: string
  ): Promise<Agent> => {
    const params = { organizationId, projectId };
    const response = await client.get<Agent>(`/api/agents/key/${key}`, { params });
    return response.data;
  },

  create: async (request: CreateAgentRequest): Promise<Agent> => {
    const response = await client.post<Agent>('/api/agents', request);
    return response.data;
  },

  update: async (id: string, request: UpdateAgentRequest): Promise<Agent> => {
    const response = await client.put<Agent>(`/api/agents/${id}`, request);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/agents/${id}`);
  },

  enable: async (id: string): Promise<Agent> => {
    const response = await client.post<Agent>(`/api/agents/${id}/enable`);
    return response.data;
  },

  disable: async (id: string): Promise<Agent> => {
    const response = await client.post<Agent>(`/api/agents/${id}/disable`);
    return response.data;
  },
};
