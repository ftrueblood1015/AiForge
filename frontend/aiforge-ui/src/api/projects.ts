import client from './client';
import type { Project, CreateProjectRequest } from '../types';

export const projectsApi = {
  getAll: async (): Promise<Project[]> => {
    const response = await client.get<Project[]>('/api/projects');
    return response.data;
  },

  getById: async (id: string): Promise<Project> => {
    const response = await client.get<Project>(`/api/projects/${id}`);
    return response.data;
  },

  getByKey: async (key: string): Promise<Project> => {
    const response = await client.get<Project>(`/api/projects/key/${key}`);
    return response.data;
  },

  create: async (request: CreateProjectRequest): Promise<Project> => {
    const response = await client.post<Project>('/api/projects', request);
    return response.data;
  },

  update: async (id: string, request: Partial<CreateProjectRequest>): Promise<Project> => {
    const response = await client.put<Project>(`/api/projects/${id}`, request);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/projects/${id}`);
  },
};
