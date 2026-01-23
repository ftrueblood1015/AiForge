import { create } from 'zustand';
import type {
  Agent,
  AgentListItem,
  AgentSearchParams,
  CreateAgentRequest,
  UpdateAgentRequest,
} from '../types';
import { agentsApi } from '../api/agents';

interface AgentState {
  agents: AgentListItem[];
  currentAgent: Agent | null;
  isLoading: boolean;
  error: string | null;
  searchParams: AgentSearchParams;

  // Actions
  fetchAgents: (params?: AgentSearchParams) => Promise<void>;
  fetchAgent: (id: string) => Promise<void>;
  createAgent: (request: CreateAgentRequest) => Promise<Agent>;
  updateAgent: (id: string, request: UpdateAgentRequest) => Promise<Agent>;
  deleteAgent: (id: string) => Promise<void>;
  enableAgent: (id: string) => Promise<void>;
  disableAgent: (id: string) => Promise<void>;
  setSearchParams: (params: AgentSearchParams) => void;
  clearCurrentAgent: () => void;
  clearError: () => void;
}

export const useAgentStore = create<AgentState>((set, get) => ({
  agents: [],
  currentAgent: null,
  isLoading: false,
  error: null,
  searchParams: {},

  fetchAgents: async (params?: AgentSearchParams) => {
    const searchParams = params || get().searchParams;
    set({ isLoading: true, error: null, searchParams });
    try {
      const response = await agentsApi.getAll(searchParams);
      set({ agents: response.items, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchAgent: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const agent = await agentsApi.getById(id);
      set({ currentAgent: agent, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createAgent: async (request: CreateAgentRequest) => {
    set({ isLoading: true, error: null });
    try {
      const agent = await agentsApi.create(request);
      // Refresh agents list after creation
      const { searchParams } = get();
      const response = await agentsApi.getAll(searchParams);
      set({ agents: response.items, isLoading: false });
      return agent;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateAgent: async (id: string, request: UpdateAgentRequest) => {
    set({ isLoading: true, error: null });
    try {
      const agent = await agentsApi.update(id, request);
      set((state) => ({
        agents: state.agents.map((a) =>
          a.id === id
            ? {
                ...a,
                name: agent.name,
                description: agent.description,
                agentType: agent.agentType,
                status: agent.status,
                isEnabled: agent.isEnabled,
              }
            : a
        ),
        currentAgent: state.currentAgent?.id === id ? agent : state.currentAgent,
        isLoading: false,
      }));
      return agent;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  deleteAgent: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      await agentsApi.delete(id);
      set((state) => ({
        agents: state.agents.filter((a) => a.id !== id),
        currentAgent: state.currentAgent?.id === id ? null : state.currentAgent,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  enableAgent: async (id: string) => {
    try {
      await agentsApi.enable(id);
      set((state) => ({
        agents: state.agents.map((a) =>
          a.id === id ? { ...a, isEnabled: true } : a
        ),
        currentAgent:
          state.currentAgent?.id === id
            ? { ...state.currentAgent, isEnabled: true }
            : state.currentAgent,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  disableAgent: async (id: string) => {
    try {
      await agentsApi.disable(id);
      set((state) => ({
        agents: state.agents.map((a) =>
          a.id === id ? { ...a, isEnabled: false } : a
        ),
        currentAgent:
          state.currentAgent?.id === id
            ? { ...state.currentAgent, isEnabled: false }
            : state.currentAgent,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  setSearchParams: (params: AgentSearchParams) => {
    set({ searchParams: params });
  },

  clearCurrentAgent: () => set({ currentAgent: null }),

  clearError: () => set({ error: null }),
}));
