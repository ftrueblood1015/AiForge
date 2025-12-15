import client from './client';
import type {
  FileChange,
  TestLink,
  TechnicalDebt,
  FileHistoryResponse,
  HotFile,
  FileCoverageResponse,
  DebtBacklogResponse,
  DebtSummaryResponse,
  LogFileChangeRequest,
  LinkTestRequest,
  UpdateTestOutcomeRequest,
  CreateDebtRequest,
  UpdateDebtRequest,
  ResolveDebtRequest,
} from '../types';

// File Change API
export const fileChangeApi = {
  logChange: async (ticketId: string, request: LogFileChangeRequest): Promise<FileChange> => {
    const response = await client.post<FileChange>(
      `/api/tickets/${ticketId}/file-changes`,
      request
    );
    return response.data;
  },

  getByTicket: async (ticketId: string): Promise<FileChange[]> => {
    const response = await client.get<FileChange[]>(`/api/tickets/${ticketId}/file-changes`);
    return response.data;
  },

  getFileHistory: async (filePath: string): Promise<FileHistoryResponse> => {
    const response = await client.get<FileHistoryResponse>(
      `/api/files/history`,
      { params: { path: filePath } }
    );
    return response.data;
  },

  getHotFiles: async (limit: number = 10): Promise<HotFile[]> => {
    const response = await client.get<HotFile[]>(
      `/api/files/hot`,
      { params: { limit } }
    );
    return response.data;
  },
};

// Test Link API
export const testLinkApi = {
  linkTest: async (ticketId: string, request: LinkTestRequest): Promise<TestLink> => {
    const response = await client.post<TestLink>(
      `/api/tickets/${ticketId}/tests`,
      request
    );
    return response.data;
  },

  getByTicket: async (ticketId: string): Promise<TestLink[]> => {
    const response = await client.get<TestLink[]>(`/api/tickets/${ticketId}/tests`);
    return response.data;
  },

  updateOutcome: async (testLinkId: string, request: UpdateTestOutcomeRequest): Promise<TestLink> => {
    const response = await client.patch<TestLink>(
      `/api/tests/${testLinkId}/outcome`,
      request
    );
    return response.data;
  },

  getFileCoverage: async (filePath: string): Promise<FileCoverageResponse> => {
    const response = await client.get<FileCoverageResponse>(
      `/api/files/coverage`,
      { params: { path: filePath } }
    );
    return response.data;
  },

  getCoverageGaps: async (): Promise<string[]> => {
    const response = await client.get<string[]>(`/api/files/coverage-gaps`);
    return response.data;
  },
};

// Technical Debt API
export const technicalDebtApi = {
  flagDebt: async (ticketId: string, request: CreateDebtRequest): Promise<TechnicalDebt> => {
    const response = await client.post<TechnicalDebt>(
      `/api/tickets/${ticketId}/debt`,
      request
    );
    return response.data;
  },

  getBacklog: async (
    status?: string,
    category?: string,
    severity?: string
  ): Promise<DebtBacklogResponse> => {
    const response = await client.get<DebtBacklogResponse>(`/api/debt`, {
      params: { status, category, severity },
    });
    return response.data;
  },

  getById: async (debtId: string): Promise<TechnicalDebt> => {
    const response = await client.get<TechnicalDebt>(`/api/debt/${debtId}`);
    return response.data;
  },

  update: async (debtId: string, request: UpdateDebtRequest): Promise<TechnicalDebt> => {
    const response = await client.patch<TechnicalDebt>(`/api/debt/${debtId}`, request);
    return response.data;
  },

  resolve: async (debtId: string, request: ResolveDebtRequest): Promise<TechnicalDebt> => {
    const response = await client.post<TechnicalDebt>(`/api/debt/${debtId}/resolve`, request);
    return response.data;
  },

  getSummary: async (): Promise<DebtSummaryResponse> => {
    const response = await client.get<DebtSummaryResponse>(`/api/debt/summary`);
    return response.data;
  },

  getByTicket: async (ticketId: string): Promise<TechnicalDebt[]> => {
    const response = await client.get<TechnicalDebt[]>(`/api/tickets/${ticketId}/debt`);
    return response.data;
  },
};
