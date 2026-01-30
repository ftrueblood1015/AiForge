import client from './client';
import type {
  Skill,
  SkillListResponse,
  CreateSkillRequest,
  UpdateSkillRequest,
  SkillSearchParams,
} from '../types';

export const skillsApi = {
  getAll: async (params?: SkillSearchParams): Promise<SkillListResponse> => {
    // Map frontend params to backend params (isPublished -> publishedOnly)
    const apiParams = params ? {
      organizationId: params.organizationId,
      projectId: params.projectId,
      category: params.category,
      publishedOnly: params.isPublished,
    } : undefined;
    const response = await client.get<SkillListResponse>('/api/skills', { params: apiParams });
    return response.data;
  },

  getById: async (id: string): Promise<Skill> => {
    const response = await client.get<Skill>(`/api/skills/${id}`);
    return response.data;
  },

  getByKey: async (
    key: string,
    organizationId: string,
    projectId?: string
  ): Promise<Skill> => {
    const params = { organizationId, projectId };
    const response = await client.get<Skill>(`/api/skills/key/${key}`, { params });
    return response.data;
  },

  create: async (request: CreateSkillRequest): Promise<Skill> => {
    const response = await client.post<Skill>('/api/skills', request);
    return response.data;
  },

  update: async (id: string, request: UpdateSkillRequest): Promise<Skill> => {
    const response = await client.put<Skill>(`/api/skills/${id}`, request);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/skills/${id}`);
  },

  publish: async (id: string): Promise<Skill> => {
    const response = await client.post<Skill>(`/api/skills/${id}/publish`);
    return response.data;
  },

  unpublish: async (id: string): Promise<Skill> => {
    const response = await client.post<Skill>(`/api/skills/${id}/unpublish`);
    return response.data;
  },
};
