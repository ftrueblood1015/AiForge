import client from './client';
import type { PlanningSession, ReasoningLog, ProgressEntry } from '../types';

export interface PlanningData {
  planningSessions: PlanningSession[];
  reasoningLogs: ReasoningLog[];
  progressEntries: ProgressEntry[];
}

export const planningApi = {
  // Get all planning data for a ticket
  getByTicket: async (ticketId: string): Promise<PlanningData> => {
    const response = await client.get<PlanningData>('/api/planning/data', {
      params: { ticketId }
    });
    return response.data;
  },

  // Planning Sessions
  startSession: async (ticketId: string, initialUnderstanding: string, assumptions?: string[]): Promise<PlanningSession> => {
    const response = await client.post<PlanningSession>('/api/planning/sessions', {
      ticketId,
      initialUnderstanding,
      assumptions,
    });
    return response.data;
  },

  completeSession: async (sessionId: string, chosenApproach: string, rationale: string): Promise<PlanningSession> => {
    const response = await client.post<PlanningSession>(`/api/planning/sessions/${sessionId}/complete`, {
      chosenApproach,
      rationale,
    });
    return response.data;
  },

  // Reasoning Logs
  logDecision: async (
    ticketId: string,
    decisionPoint: string,
    chosenOption: string,
    rationale: string,
    optionsConsidered?: string[],
    confidencePercent?: number
  ): Promise<ReasoningLog> => {
    const response = await client.post<ReasoningLog>('/api/planning/reasoning', {
      ticketId,
      decisionPoint,
      chosenOption,
      rationale,
      optionsConsidered,
      confidencePercent,
    });
    return response.data;
  },

  // Progress Entries
  logProgress: async (
    ticketId: string,
    content: string,
    outcome: 'Success' | 'Failure' | 'Partial' | 'Blocked',
    filesAffected?: string[],
    errorDetails?: string
  ): Promise<ProgressEntry> => {
    const response = await client.post<ProgressEntry>('/api/planning/progress', {
      ticketId,
      content,
      outcome,
      filesAffected,
      errorDetails,
    });
    return response.data;
  },
};
