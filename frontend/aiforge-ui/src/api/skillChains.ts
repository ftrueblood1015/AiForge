import client from './client';
import type {
  SkillChain,
  SkillChainSummary,
  SkillChainLink,
  SkillChainExecution,
  SkillChainExecutionSummary,
  SkillChainLinkExecution,
  CreateSkillChainRequest,
  UpdateSkillChainRequest,
  CreateSkillChainLinkRequest,
  UpdateSkillChainLinkRequest,
  ReorderLinksRequest,
  StartChainExecutionRequest,
  RecordLinkOutcomeRequest,
  ResumeExecutionRequest,
  ResolveInterventionRequest,
  PauseExecutionRequest,
  CancelExecutionRequest,
  SkillChainSearchParams,
  SkillChainExecutionSearchParams,
} from '../types';

// ===========================================
// Skill Chain CRUD API
// ===========================================

export const skillChainsApi = {
  // List skill chains with optional filters
  getAll: async (params?: SkillChainSearchParams): Promise<SkillChainSummary[]> => {
    const response = await client.get<SkillChainSummary[]>('/api/skillchains', { params });
    return response.data;
  },

  // Get skill chain by ID
  getById: async (id: string): Promise<SkillChain> => {
    const response = await client.get<SkillChain>(`/api/skillchains/${id}`);
    return response.data;
  },

  // Get skill chain by key with scope resolution
  getByKey: async (
    key: string,
    organizationId: string,
    projectId?: string
  ): Promise<SkillChain> => {
    const params = { organizationId, projectId };
    const response = await client.get<SkillChain>(`/api/skillchains/key/${key}`, { params });
    return response.data;
  },

  // Create a new skill chain
  create: async (request: CreateSkillChainRequest): Promise<SkillChain> => {
    const response = await client.post<SkillChain>('/api/skillchains', request);
    return response.data;
  },

  // Update an existing skill chain
  update: async (id: string, request: UpdateSkillChainRequest): Promise<SkillChain> => {
    const response = await client.put<SkillChain>(`/api/skillchains/${id}`, request);
    return response.data;
  },

  // Delete a skill chain
  delete: async (id: string): Promise<void> => {
    await client.delete(`/api/skillchains/${id}`);
  },

  // Publish a skill chain
  publish: async (id: string): Promise<SkillChain> => {
    const response = await client.post<SkillChain>(`/api/skillchains/${id}/publish`);
    return response.data;
  },

  // Unpublish a skill chain
  unpublish: async (id: string): Promise<SkillChain> => {
    const response = await client.post<SkillChain>(`/api/skillchains/${id}/unpublish`);
    return response.data;
  },

  // ===========================================
  // Link Management
  // ===========================================

  // Add a link to a skill chain
  addLink: async (chainId: string, request: CreateSkillChainLinkRequest): Promise<SkillChainLink> => {
    const response = await client.post<SkillChainLink>(`/api/skillchains/${chainId}/links`, request);
    return response.data;
  },

  // Update a chain link
  updateLink: async (linkId: string, request: UpdateSkillChainLinkRequest): Promise<SkillChainLink> => {
    const response = await client.put<SkillChainLink>(`/api/skillchains/links/${linkId}`, request);
    return response.data;
  },

  // Remove a chain link
  removeLink: async (linkId: string): Promise<void> => {
    await client.delete(`/api/skillchains/links/${linkId}`);
  },

  // Reorder links in a chain
  reorderLinks: async (chainId: string, request: ReorderLinksRequest): Promise<void> => {
    await client.put(`/api/skillchains/${chainId}/links/reorder`, request);
  },
};

// ===========================================
// Skill Chain Execution API
// ===========================================

export const skillChainExecutionsApi = {
  // List executions with optional filters
  getAll: async (params?: SkillChainExecutionSearchParams): Promise<SkillChainExecutionSummary[]> => {
    const response = await client.get<SkillChainExecutionSummary[]>('/api/skillchain-executions', { params });
    return response.data;
  },

  // Get execution by ID
  getById: async (id: string): Promise<SkillChainExecution> => {
    const response = await client.get<SkillChainExecution>(`/api/skillchain-executions/${id}`);
    return response.data;
  },

  // Start a new chain execution
  start: async (request: StartChainExecutionRequest): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>('/api/skillchain-executions', request);
    return response.data;
  },

  // Pause a running execution
  pause: async (id: string, request: PauseExecutionRequest): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>(`/api/skillchain-executions/${id}/pause`, request);
    return response.data;
  },

  // Resume a paused execution
  resume: async (id: string, request?: ResumeExecutionRequest): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>(`/api/skillchain-executions/${id}/resume`, request || {});
    return response.data;
  },

  // Cancel an execution
  cancel: async (id: string, request: CancelExecutionRequest): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>(`/api/skillchain-executions/${id}/cancel`, request);
    return response.data;
  },

  // Record the outcome of a link execution
  recordOutcome: async (id: string, request: RecordLinkOutcomeRequest): Promise<SkillChainLinkExecution> => {
    const response = await client.post<SkillChainLinkExecution>(`/api/skillchain-executions/${id}/record-outcome`, request);
    return response.data;
  },

  // Advance execution to the next link
  advance: async (id: string): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>(`/api/skillchain-executions/${id}/advance`);
    return response.data;
  },

  // Get executions requiring human intervention
  getPendingInterventions: async (projectId?: string): Promise<SkillChainExecutionSummary[]> => {
    const params = projectId ? { projectId } : undefined;
    const response = await client.get<SkillChainExecutionSummary[]>('/api/skillchain-executions/pending-interventions', { params });
    return response.data;
  },

  // Resolve a human intervention
  resolveIntervention: async (id: string, request: ResolveInterventionRequest): Promise<SkillChainExecution> => {
    const response = await client.post<SkillChainExecution>(`/api/skillchain-executions/${id}/resolve`, request);
    return response.data;
  },
};
