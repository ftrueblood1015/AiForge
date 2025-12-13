import client from './client';
import type {
  ImplementationPlan,
  CreateImplementationPlanRequest,
  UpdateImplementationPlanRequest,
  ApproveImplementationPlanRequest,
  RejectImplementationPlanRequest,
  SupersedeImplementationPlanRequest,
} from '../types';

export const plansApi = {
  // Get all plans for a ticket
  getByTicket: async (ticketId: string): Promise<ImplementationPlan[]> => {
    const response = await client.get<ImplementationPlan[]>(`/api/tickets/${ticketId}/plans`);
    return response.data;
  },

  // Get current plan for a ticket (latest approved or draft)
  getCurrent: async (ticketId: string): Promise<ImplementationPlan | null> => {
    try {
      const response = await client.get<ImplementationPlan>(`/api/tickets/${ticketId}/plans/current`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  // Get approved plan for a ticket
  getApproved: async (ticketId: string): Promise<ImplementationPlan | null> => {
    try {
      const response = await client.get<ImplementationPlan>(`/api/tickets/${ticketId}/plans/approved`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  // Get plan by ID
  getById: async (planId: string): Promise<ImplementationPlan> => {
    const response = await client.get<ImplementationPlan>(`/api/plans/${planId}`);
    return response.data;
  },

  // Create new plan for a ticket
  create: async (ticketId: string, request: CreateImplementationPlanRequest): Promise<ImplementationPlan> => {
    const response = await client.post<ImplementationPlan>(`/api/tickets/${ticketId}/plans`, request);
    return response.data;
  },

  // Update a draft plan
  update: async (planId: string, request: UpdateImplementationPlanRequest): Promise<ImplementationPlan> => {
    const response = await client.put<ImplementationPlan>(`/api/plans/${planId}`, request);
    return response.data;
  },

  // Delete a draft plan
  delete: async (planId: string): Promise<void> => {
    await client.delete(`/api/plans/${planId}`);
  },

  // Approve a draft plan
  approve: async (planId: string, request: ApproveImplementationPlanRequest = {}): Promise<ImplementationPlan> => {
    const response = await client.post<ImplementationPlan>(`/api/plans/${planId}/approve`, request);
    return response.data;
  },

  // Reject a draft plan
  reject: async (planId: string, request: RejectImplementationPlanRequest = {}): Promise<ImplementationPlan> => {
    const response = await client.post<ImplementationPlan>(`/api/plans/${planId}/reject`, request);
    return response.data;
  },

  // Supersede an approved plan with a new version
  supersede: async (planId: string, request: SupersedeImplementationPlanRequest): Promise<ImplementationPlan> => {
    const response = await client.post<ImplementationPlan>(`/api/plans/${planId}/supersede`, request);
    return response.data;
  },
};
