import { create } from 'zustand';
import type { Ticket, TicketSearchParams, TicketStatus } from '../types';
import { ticketsApi } from '../api/tickets';

interface TicketState {
  tickets: Ticket[];
  currentTicket: Ticket | null;
  isLoading: boolean;
  error: string | null;
  searchParams: TicketSearchParams;

  // Actions
  fetchTickets: (params?: TicketSearchParams) => Promise<void>;
  fetchTicket: (keyOrId: string) => Promise<void>;
  createTicket: (projectId: string, title: string, type: string, description?: string, priority?: string) => Promise<Ticket>;
  updateTicketStatus: (ticketId: string, status: TicketStatus) => Promise<void>;
  setSearchParams: (params: TicketSearchParams) => void;
  clearError: () => void;
}

export const useTicketStore = create<TicketState>((set, get) => ({
  tickets: [],
  currentTicket: null,
  isLoading: false,
  error: null,
  searchParams: {},

  fetchTickets: async (params?: TicketSearchParams) => {
    const searchParams = params || get().searchParams;
    set({ isLoading: true, error: null, searchParams });
    try {
      const tickets = await ticketsApi.search(searchParams);
      set({ tickets, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  fetchTicket: async (keyOrId: string) => {
    set({ isLoading: true, error: null });
    try {
      const ticket = await ticketsApi.getByKey(keyOrId);
      set({ currentTicket: ticket, isLoading: false });
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
    }
  },

  createTicket: async (projectId: string, title: string, type: string, description?: string, priority?: string) => {
    set({ isLoading: true, error: null });
    try {
      const ticket = await ticketsApi.create({
        projectId,
        title,
        type: type as Ticket['type'],
        description,
        priority: priority as Ticket['priority'],
      });
      set((state) => ({
        tickets: [...state.tickets, ticket],
        isLoading: false
      }));
      return ticket;
    } catch (error) {
      set({ error: (error as Error).message, isLoading: false });
      throw error;
    }
  },

  updateTicketStatus: async (ticketId: string, status: TicketStatus) => {
    try {
      await ticketsApi.transition(ticketId, { status });
      set((state) => ({
        tickets: state.tickets.map((t) =>
          t.id === ticketId ? { ...t, status } : t
        ),
        currentTicket: state.currentTicket?.id === ticketId
          ? { ...state.currentTicket, status }
          : state.currentTicket,
      }));
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },

  setSearchParams: (params: TicketSearchParams) => {
    set({ searchParams: params });
  },

  clearError: () => set({ error: null }),
}));
