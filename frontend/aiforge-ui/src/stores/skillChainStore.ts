import { create } from 'zustand';
import type {
  SkillChain,
  SkillChainSummary,
  SkillChainLink,
  SkillChainSearchParams,
  CreateSkillChainRequest,
  UpdateSkillChainRequest,
  CreateSkillChainLinkRequest,
  UpdateSkillChainLinkRequest,
} from '../types';
import { skillChainsApi } from '../api/skillChains';

interface SkillChainState {
  chains: SkillChainSummary[];
  currentChain: SkillChain | null;
  isLoading: boolean;
  error: string | null;
  searchParams: SkillChainSearchParams;

  // Chain actions
  fetchChains: (params?: SkillChainSearchParams) => Promise<void>;
  fetchChain: (id: string) => Promise<void>;
  createChain: (request: CreateSkillChainRequest) => Promise<SkillChain>;
  updateChain: (id: string, request: UpdateSkillChainRequest) => Promise<SkillChain>;
  deleteChain: (id: string) => Promise<void>;
  publishChain: (id: string) => Promise<void>;
  unpublishChain: (id: string) => Promise<void>;

  // Link actions
  addLink: (chainId: string, request: CreateSkillChainLinkRequest) => Promise<SkillChainLink>;
  updateLink: (linkId: string, request: UpdateSkillChainLinkRequest) => Promise<SkillChainLink>;
  removeLink: (chainId: string, linkId: string) => Promise<void>;
  reorderLinks: (chainId: string, linkIds: string[]) => Promise<void>;

  // Utility actions
  setSearchParams: (params: SkillChainSearchParams) => void;
  clearCurrentChain: () => void;
  clearError: () => void;
}

export const useSkillChainStore = create<SkillChainState>((set, get) => ({
  chains: [],
  currentChain: null,
  isLoading: false,
  error: null,
  searchParams: {},

  fetchChains: async (params?: SkillChainSearchParams) => {
    const searchParams = params || get().searchParams;
    set({ isLoading: true, error: null, searchParams });
    try {
      const chains = await skillChainsApi.getAll(searchParams);
      set({ chains, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchChain: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const chain = await skillChainsApi.getById(id);
      set({ currentChain: chain, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createChain: async (request: CreateSkillChainRequest) => {
    set({ isLoading: true, error: null });
    try {
      const chain = await skillChainsApi.create(request);
      // Refresh chains list after creation
      const { searchParams } = get();
      const chains = await skillChainsApi.getAll(searchParams);
      set({ chains, isLoading: false });
      return chain;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateChain: async (id: string, request: UpdateSkillChainRequest) => {
    set({ isLoading: true, error: null });
    try {
      const chain = await skillChainsApi.update(id, request);
      set((state) => ({
        chains: state.chains.map((c) =>
          c.id === id
            ? {
                ...c,
                name: chain.name,
                description: chain.description,
                isPublished: chain.isPublished,
              }
            : c
        ),
        currentChain: state.currentChain?.id === id ? chain : state.currentChain,
        isLoading: false,
      }));
      return chain;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  deleteChain: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      await skillChainsApi.delete(id);
      set((state) => ({
        chains: state.chains.filter((c) => c.id !== id),
        currentChain: state.currentChain?.id === id ? null : state.currentChain,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  publishChain: async (id: string) => {
    try {
      await skillChainsApi.publish(id);
      set((state) => ({
        chains: state.chains.map((c) =>
          c.id === id ? { ...c, isPublished: true } : c
        ),
        currentChain:
          state.currentChain?.id === id
            ? { ...state.currentChain, isPublished: true }
            : state.currentChain,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  unpublishChain: async (id: string) => {
    try {
      await skillChainsApi.unpublish(id);
      set((state) => ({
        chains: state.chains.map((c) =>
          c.id === id ? { ...c, isPublished: false } : c
        ),
        currentChain:
          state.currentChain?.id === id
            ? { ...state.currentChain, isPublished: false }
            : state.currentChain,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  addLink: async (chainId: string, request: CreateSkillChainLinkRequest) => {
    set({ isLoading: true, error: null });
    try {
      const link = await skillChainsApi.addLink(chainId, request);
      // Refresh current chain to get updated links
      if (get().currentChain?.id === chainId) {
        const chain = await skillChainsApi.getById(chainId);
        set({ currentChain: chain, isLoading: false });
      } else {
        set({ isLoading: false });
      }
      // Update link count in chains list
      set((state) => ({
        chains: state.chains.map((c) =>
          c.id === chainId ? { ...c, linkCount: c.linkCount + 1 } : c
        ),
      }));
      return link;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateLink: async (linkId: string, request: UpdateSkillChainLinkRequest) => {
    set({ isLoading: true, error: null });
    try {
      const link = await skillChainsApi.updateLink(linkId, request);
      // Update link in current chain if loaded
      set((state) => ({
        currentChain: state.currentChain
          ? {
              ...state.currentChain,
              links: state.currentChain.links.map((l) =>
                l.id === linkId ? { ...l, ...link } : l
              ),
            }
          : null,
        isLoading: false,
      }));
      return link;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  removeLink: async (chainId: string, linkId: string) => {
    set({ isLoading: true, error: null });
    try {
      await skillChainsApi.removeLink(linkId);
      // Update current chain if loaded
      set((state) => ({
        currentChain: state.currentChain
          ? {
              ...state.currentChain,
              links: state.currentChain.links.filter((l) => l.id !== linkId),
            }
          : null,
        chains: state.chains.map((c) =>
          c.id === chainId ? { ...c, linkCount: Math.max(0, c.linkCount - 1) } : c
        ),
        isLoading: false,
      }));
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  reorderLinks: async (chainId: string, linkIds: string[]) => {
    set({ isLoading: true, error: null });
    try {
      await skillChainsApi.reorderLinks(chainId, { linkIdsInOrder: linkIds });
      // Refresh current chain to get updated link positions
      if (get().currentChain?.id === chainId) {
        const chain = await skillChainsApi.getById(chainId);
        set({ currentChain: chain, isLoading: false });
      } else {
        set({ isLoading: false });
      }
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  setSearchParams: (params: SkillChainSearchParams) => {
    set({ searchParams: params });
  },

  clearCurrentChain: () => set({ currentChain: null }),

  clearError: () => set({ error: null }),
}));
