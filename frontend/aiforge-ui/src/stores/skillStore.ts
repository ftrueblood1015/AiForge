import { create } from 'zustand';
import type {
  Skill,
  SkillListItem,
  SkillSearchParams,
  CreateSkillRequest,
  UpdateSkillRequest,
} from '../types';
import { skillsApi } from '../api/skills';

interface SkillState {
  skills: SkillListItem[];
  currentSkill: Skill | null;
  isLoading: boolean;
  error: string | null;
  searchParams: SkillSearchParams;

  // Actions
  fetchSkills: (params?: SkillSearchParams) => Promise<void>;
  fetchSkill: (id: string) => Promise<void>;
  createSkill: (request: CreateSkillRequest) => Promise<Skill>;
  updateSkill: (id: string, request: UpdateSkillRequest) => Promise<Skill>;
  deleteSkill: (id: string) => Promise<void>;
  publishSkill: (id: string) => Promise<void>;
  unpublishSkill: (id: string) => Promise<void>;
  setSearchParams: (params: SkillSearchParams) => void;
  clearCurrentSkill: () => void;
  clearError: () => void;
}

export const useSkillStore = create<SkillState>((set, get) => ({
  skills: [],
  currentSkill: null,
  isLoading: false,
  error: null,
  searchParams: {},

  fetchSkills: async (params?: SkillSearchParams) => {
    const searchParams = params || get().searchParams;
    set({ isLoading: true, error: null, searchParams });
    try {
      const response = await skillsApi.getAll(searchParams);
      set({ skills: response.items, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchSkill: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const skill = await skillsApi.getById(id);
      set({ currentSkill: skill, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createSkill: async (request: CreateSkillRequest) => {
    set({ isLoading: true, error: null });
    try {
      const skill = await skillsApi.create(request);
      // Refresh skills list after creation
      const { searchParams } = get();
      const response = await skillsApi.getAll(searchParams);
      set({ skills: response.items, isLoading: false });
      return skill;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateSkill: async (id: string, request: UpdateSkillRequest) => {
    set({ isLoading: true, error: null });
    try {
      const skill = await skillsApi.update(id, request);
      set((state) => ({
        skills: state.skills.map((s) =>
          s.id === id
            ? {
                ...s,
                name: skill.name,
                description: skill.description,
                category: skill.category,
                isPublished: skill.isPublished,
              }
            : s
        ),
        currentSkill: state.currentSkill?.id === id ? skill : state.currentSkill,
        isLoading: false,
      }));
      return skill;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  deleteSkill: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      await skillsApi.delete(id);
      set((state) => ({
        skills: state.skills.filter((s) => s.id !== id),
        currentSkill: state.currentSkill?.id === id ? null : state.currentSkill,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  publishSkill: async (id: string) => {
    try {
      await skillsApi.publish(id);
      set((state) => ({
        skills: state.skills.map((s) =>
          s.id === id ? { ...s, isPublished: true } : s
        ),
        currentSkill:
          state.currentSkill?.id === id
            ? { ...state.currentSkill, isPublished: true }
            : state.currentSkill,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  unpublishSkill: async (id: string) => {
    try {
      await skillsApi.unpublish(id);
      set((state) => ({
        skills: state.skills.map((s) =>
          s.id === id ? { ...s, isPublished: false } : s
        ),
        currentSkill:
          state.currentSkill?.id === id
            ? { ...state.currentSkill, isPublished: false }
            : state.currentSkill,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  setSearchParams: (params: SkillSearchParams) => {
    set({ searchParams: params });
  },

  clearCurrentSkill: () => set({ currentSkill: null }),

  clearError: () => set({ error: null }),
}));
