import client from './client';
import type {
  ProjectMember,
  AddProjectMemberRequest,
  UpdateProjectMemberRoleRequest,
  UserSearchResult,
} from '../types';

export const projectMembersApi = {
  /**
   * Get all members of a project
   */
  getMembers: async (projectId: string): Promise<ProjectMember[]> => {
    const response = await client.get<ProjectMember[]>(`/api/projects/${projectId}/members`);
    return response.data;
  },

  /**
   * Get a specific member of a project
   */
  getMember: async (projectId: string, userId: string): Promise<ProjectMember> => {
    const response = await client.get<ProjectMember>(`/api/projects/${projectId}/members/${userId}`);
    return response.data;
  },

  /**
   * Get the current user's membership in a project
   */
  getMyMembership: async (projectId: string): Promise<ProjectMember | null> => {
    try {
      const response = await client.get<ProjectMember>(`/api/projects/${projectId}/members/me`);
      return response.data;
    } catch (error: unknown) {
      // Return null if not a member (404)
      if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as { response?: { status?: number } };
        if (axiosError.response?.status === 404) {
          return null;
        }
      }
      throw error;
    }
  },

  /**
   * Add a member to a project (requires Owner role)
   */
  addMember: async (projectId: string, request: AddProjectMemberRequest): Promise<void> => {
    await client.post(`/api/projects/${projectId}/members`, request);
  },

  /**
   * Update a member's role (requires Owner role)
   */
  updateMemberRole: async (
    projectId: string,
    userId: string,
    request: UpdateProjectMemberRoleRequest
  ): Promise<void> => {
    await client.put(`/api/projects/${projectId}/members/${userId}`, request);
  },

  /**
   * Remove a member from a project (requires Owner role)
   */
  removeMember: async (projectId: string, userId: string): Promise<void> => {
    await client.delete(`/api/projects/${projectId}/members/${userId}`);
  },

  /**
   * Search users to add as project members
   */
  searchUsers: async (query: string, excludeProjectId?: string): Promise<UserSearchResult[]> => {
    const params = new URLSearchParams({ query });
    if (excludeProjectId) {
      params.append('excludeProjectId', excludeProjectId);
    }
    const response = await client.get<UserSearchResult[]>(`/api/users/search?${params.toString()}`);
    return response.data;
  },
};
