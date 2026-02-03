import { create } from 'zustand';
import type { ProjectMember, ProjectRole } from '../types';
import { projectMembersApi } from '../api/projectMembers';

interface ProjectMemberState {
  members: ProjectMember[];
  currentUserRole: ProjectRole | null;
  currentUserMembership: ProjectMember | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchMembers: (projectId: string) => Promise<void>;
  fetchMyMembership: (projectId: string) => Promise<void>;
  addMember: (projectId: string, email: string, role: ProjectRole) => Promise<void>;
  updateMemberRole: (projectId: string, userId: string, role: ProjectRole) => Promise<void>;
  removeMember: (projectId: string, userId: string) => Promise<void>;
  clearMembers: () => void;
  clearError: () => void;
}

export const useProjectMemberStore = create<ProjectMemberState>((set, get) => ({
  members: [],
  currentUserRole: null,
  currentUserMembership: null,
  isLoading: false,
  error: null,

  fetchMembers: async (projectId: string) => {
    set({ isLoading: true, error: null });
    try {
      const members = await projectMembersApi.getMembers(projectId);
      set({ members, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchMyMembership: async (projectId: string) => {
    try {
      const membership = await projectMembersApi.getMyMembership(projectId);
      set({
        currentUserMembership: membership,
        currentUserRole: membership?.role ?? null,
      });
    } catch (error) {
      set({
        currentUserMembership: null,
        currentUserRole: null,
      });
    }
  },

  addMember: async (projectId: string, email: string, role: ProjectRole) => {
    set({ isLoading: true, error: null });
    try {
      await projectMembersApi.addMember(projectId, { email, role });
      // Refresh members list
      const members = await projectMembersApi.getMembers(projectId);
      set({ members, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateMemberRole: async (projectId: string, userId: string, role: ProjectRole) => {
    set({ isLoading: true, error: null });
    try {
      await projectMembersApi.updateMemberRole(projectId, userId, { role });
      // Update local state
      set((state) => ({
        members: state.members.map((m) =>
          m.userId === userId ? { ...m, role } : m
        ),
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  removeMember: async (projectId: string, userId: string) => {
    set({ isLoading: true, error: null });
    try {
      await projectMembersApi.removeMember(projectId, userId);
      // Update local state
      set((state) => ({
        members: state.members.filter((m) => m.userId !== userId),
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  clearMembers: () => set({
    members: [],
    currentUserRole: null,
    currentUserMembership: null,
  }),

  clearError: () => set({ error: null }),
}));
