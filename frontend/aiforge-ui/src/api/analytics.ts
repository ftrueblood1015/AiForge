import client from './client';
import type {
  AnalyticsDashboard,
  LowConfidenceDecision,
  ConfidenceTrend,
  TicketConfidenceSummary,
  AnalyticsHotFile,
  FileCorrelation,
  RecurringIssue,
  DebtPatternSummary,
  ProductivityMetrics,
  TicketSessionAnalytics,
} from '../types';

// Analytics API
export const analyticsApi = {
  // Dashboard
  getDashboard: async (params?: {
    projectId?: string;
    startDate?: string;
    endDate?: string;
    recentActivityLimit?: number;
    topHotFilesLimit?: number;
    lowConfidenceLimit?: number;
  }): Promise<AnalyticsDashboard> => {
    const response = await client.get<AnalyticsDashboard>('/api/analytics/dashboard', {
      params,
    });
    return response.data;
  },

  // Confidence Tracking
  getLowConfidenceDecisions: async (params?: {
    confidenceThreshold?: number;
    projectId?: string;
    since?: string;
    limit?: number;
  }): Promise<LowConfidenceDecision[]> => {
    const response = await client.get<LowConfidenceDecision[]>('/api/analytics/confidence/low', {
      params,
    });
    return response.data;
  },

  getConfidenceTrends: async (params?: {
    projectId?: string;
    startDate?: string;
    endDate?: string;
    granularity?: string;
  }): Promise<ConfidenceTrend> => {
    const response = await client.get<ConfidenceTrend>('/api/analytics/confidence/trends', {
      params,
    });
    return response.data;
  },

  getLowConfidenceTickets: async (params?: {
    projectId?: string;
    confidenceThreshold?: number;
    limit?: number;
  }): Promise<TicketConfidenceSummary[]> => {
    const response = await client.get<TicketConfidenceSummary[]>('/api/analytics/confidence/tickets', {
      params,
    });
    return response.data;
  },

  // Pattern Detection
  getHotFiles: async (params?: {
    projectId?: string;
    since?: string;
    topN?: number;
  }): Promise<AnalyticsHotFile[]> => {
    const response = await client.get<AnalyticsHotFile[]>('/api/analytics/patterns/hot-files', {
      params,
    });
    return response.data;
  },

  getCorrelatedFiles: async (
    filePath: string,
    params?: {
      minCooccurrence?: number;
      topN?: number;
    }
  ): Promise<FileCorrelation> => {
    const response = await client.get<FileCorrelation>('/api/analytics/patterns/correlations', {
      params: { filePath, ...params },
    });
    return response.data;
  },

  getRecurringIssues: async (params?: {
    projectId?: string;
    ticketType?: string;
    minOccurrences?: number;
  }): Promise<RecurringIssue[]> => {
    const response = await client.get<RecurringIssue[]>('/api/analytics/patterns/recurring', {
      params,
    });
    return response.data;
  },

  getDebtPatternSummary: async (params?: {
    projectId?: string;
    includeResolved?: boolean;
  }): Promise<DebtPatternSummary> => {
    const response = await client.get<DebtPatternSummary>('/api/analytics/patterns/debt', {
      params,
    });
    return response.data;
  },

  // Session Analytics
  getTicketSessionAnalytics: async (ticketId: string): Promise<TicketSessionAnalytics> => {
    const response = await client.get<TicketSessionAnalytics>(
      `/api/analytics/sessions/ticket/${ticketId}`
    );
    return response.data;
  },

  getProductivityMetrics: async (params?: {
    projectId?: string;
    startDate?: string;
    endDate?: string;
  }): Promise<ProductivityMetrics> => {
    const response = await client.get<ProductivityMetrics>('/api/analytics/productivity', {
      params,
    });
    return response.data;
  },
};
