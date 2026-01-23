import client from './client';
import type {
  PromptTemplate,
  PromptTemplateListResponse,
  CreatePromptTemplateRequest,
  UpdatePromptTemplateRequest,
  RenderTemplateRequest,
  RenderTemplateResponse,
  PromptTemplateSearchParams,
} from '../types';

export const templatesApi = {
  getAll: async (params?: PromptTemplateSearchParams): Promise<PromptTemplateListResponse> => {
    const response = await client.get<PromptTemplateListResponse>('/api/templates', { params });
    return response.data;
  },

  getById: async (id: string): Promise<PromptTemplate> => {
    const response = await client.get<PromptTemplate>(`/api/templates/${id}`);
    return response.data;
  },

  getByKey: async (
    key: string,
    organizationId: string,
    projectId?: string
  ): Promise<PromptTemplate> => {
    const params = { organizationId, projectId };
    const response = await client.get<PromptTemplate>(`/api/templates/key/${key}`, { params });
    return response.data;
  },

  create: async (request: CreatePromptTemplateRequest): Promise<PromptTemplate> => {
    const response = await client.post<PromptTemplate>('/api/templates', request);
    return response.data;
  },

  update: async (id: string, request: UpdatePromptTemplateRequest): Promise<PromptTemplate> => {
    const response = await client.put<PromptTemplate>(`/api/templates/${id}`, request);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/templates/${id}`);
  },

  render: async (id: string, request: RenderTemplateRequest): Promise<RenderTemplateResponse> => {
    const response = await client.post<RenderTemplateResponse>(
      `/api/templates/${id}/render`,
      request
    );
    return response.data;
  },

  renderByKey: async (
    key: string,
    organizationId: string,
    request: RenderTemplateRequest,
    projectId?: string
  ): Promise<RenderTemplateResponse> => {
    const params = { organizationId, projectId };
    const response = await client.post<RenderTemplateResponse>(
      `/api/templates/key/${key}/render`,
      request,
      { params }
    );
    return response.data;
  },
};
