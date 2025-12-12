import { create } from 'zustand';
import type { Project } from '../types';
import { projectsApi } from '../api/projects';

interface ProjectState {
  projects: Project[];
  currentProject: Project | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchProjects: () => Promise<void>;
  fetchProject: (keyOrId: string) => Promise<void>;
  createProject: (key: string, name: string, description?: string) => Promise<Project>;
  clearError: () => void;
}

export const useProjectStore = create<ProjectState>((set) => ({
  projects: [],
  currentProject: null,
  isLoading: false,
  error: null,

  fetchProjects: async () => {
    set({ isLoading: true, error: null });
    try {
      const projects = await projectsApi.getAll();
      set({ projects, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchProject: async (keyOrId: string) => {
    set({ isLoading: true, error: null });
    try {
      const project = await projectsApi.getByKey(keyOrId);
      set({ currentProject: project, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createProject: async (key: string, name: string, description?: string) => {
    set({ isLoading: true, error: null });
    try {
      const project = await projectsApi.create({ key, name, description });
      set((state) => ({
        projects: [...state.projects, project],
        isLoading: false
      }));
      return project;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  clearError: () => set({ error: null }),
}));
