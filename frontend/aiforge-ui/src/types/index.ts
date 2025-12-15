// Enums
export type TicketStatus = 'ToDo' | 'InProgress' | 'InReview' | 'Done';
export type TicketType = 'Task' | 'Bug' | 'Feature' | 'Enhancement';
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';
export type HandoffType = 'SessionEnd' | 'Blocker' | 'Milestone' | 'ContextDump';
export type ProgressOutcome = 'Success' | 'Failure' | 'Partial' | 'Blocked';
export type PlanStatus = 'Draft' | 'Approved' | 'Superseded' | 'Rejected';
export type ComplexityLevel = 'Low' | 'Medium' | 'High' | 'VeryHigh';
export type EffortSize = 'XSmall' | 'Small' | 'Medium' | 'Large' | 'XLarge';

// Core Entities
export interface Project {
  id: string;
  key: string;
  name: string;
  description: string | null;
  ticketCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Ticket {
  id: string;
  projectId: string;
  key: string;
  number: number;
  title: string;
  description: string | null;
  type: TicketType;
  status: TicketStatus;
  priority: Priority;
  parentTicketId: string | null;
  currentHandoffSummary: string | null;
  createdAt: string;
  updatedAt: string;
  project?: Project;
  subTickets?: Ticket[];
  comments?: Comment[];
}

export interface Comment {
  id: string;
  ticketId: string;
  content: string;
  isAiGenerated: boolean;
  sessionId: string | null;
  createdAt: string;
}

export interface TicketHistory {
  id: string;
  ticketId: string;
  field: string;
  oldValue: string | null;
  newValue: string | null;
  changedBy: string | null;
  changedAt: string;
}

// AI/Planning Entities
export interface PlanningSession {
  id: string;
  ticketId: string;
  sessionId: string | null;
  initialUnderstanding: string;
  assumptions: string[];
  alternativesConsidered: string[];
  chosenApproach: string | null;
  rationale: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface ReasoningLog {
  id: string;
  ticketId: string;
  sessionId: string | null;
  decisionPoint: string;
  optionsConsidered: string[];
  chosenOption: string;
  rationale: string;
  confidencePercent: number | null;
  createdAt: string;
}

export interface ProgressEntry {
  id: string;
  ticketId: string;
  sessionId: string | null;
  content: string;
  outcome: ProgressOutcome;
  filesAffected: string[];
  errorDetails: string | null;
  createdAt: string;
}

export interface HandoffDocument {
  id: string;
  ticketId: string;
  sessionId: string | null;
  title: string;
  type: HandoffType;
  summary: string;
  content: string;
  structuredContext: StructuredContext | null;
  isActive: boolean;
  supersededById: string | null;
  createdAt: string;
  fileSnapshots?: FileSnapshot[];
}

export interface StructuredContext {
  assumptions?: string[];
  decisionsMade?: { decision: string; rationale: string }[];
  openQuestions?: string[];
  blockers?: string[];
  filesModified?: string[];
  testsAdded?: string[];
  nextSteps?: string[];
  warnings?: string[];
}

export interface FileSnapshot {
  id: string;
  handoffId: string;
  filePath: string;
  contentBefore: string | null;
  contentAfter: string | null;
  language: string;
  createdAt: string;
}

export interface ImplementationPlan {
  id: string;
  ticketId: string;
  content: string;
  status: PlanStatus;
  version: number;
  estimatedEffort: string | null;
  affectedFiles: string[];
  dependencyTicketIds: string[];
  createdBy: string | null;
  createdAt: string;
  approvedBy: string | null;
  approvedAt: string | null;
  supersededById: string | null;
  supersededAt: string | null;
  rejectedBy: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
}

// API Request/Response Types
export interface CreateProjectRequest {
  key: string;
  name: string;
  description?: string;
}

export interface CreateTicketRequest {
  projectId: string;
  title: string;
  description?: string;
  type: TicketType;
  priority?: Priority;
  parentTicketId?: string;
}

export interface UpdateTicketRequest {
  title?: string;
  description?: string;
  type?: TicketType;
  priority?: Priority;
}

export interface TransitionTicketRequest {
  status: TicketStatus;
  comment?: string;
}

export interface TicketSearchParams {
  projectId?: string;
  status?: TicketStatus;
  type?: TicketType;
  priority?: Priority;
  search?: string;
}

// Dashboard Stats
export interface DashboardStats {
  totalProjects: number;
  totalTickets: number;
  ticketsByStatus: Record<TicketStatus, number>;
  recentTickets: Ticket[];
}

// Implementation Plan Request Types
export interface CreateImplementationPlanRequest {
  content: string;
  estimatedEffort?: string;
  affectedFiles?: string[];
  dependencyTicketIds?: string[];
  createdBy?: string;
}

export interface UpdateImplementationPlanRequest {
  content?: string;
  estimatedEffort?: string;
  affectedFiles?: string[];
  dependencyTicketIds?: string[];
}

export interface ApproveImplementationPlanRequest {
  approvedBy?: string;
}

export interface RejectImplementationPlanRequest {
  rejectedBy?: string;
  rejectionReason?: string;
}

export interface SupersedeImplementationPlanRequest {
  content: string;
  estimatedEffort?: string;
  affectedFiles?: string[];
  dependencyTicketIds?: string[];
  createdBy?: string;
}

// Effort Estimation Types
export interface EffortEstimation {
  id: string;
  ticketId: string;
  complexity: ComplexityLevel;
  estimatedEffort: EffortSize;
  confidencePercent: number;
  estimationReasoning: string | null;
  assumptions: string | null;
  actualEffort: EffortSize | null;
  varianceNotes: string | null;
  version: number;
  revisionReason: string | null;
  sessionId: string | null;
  isLatest: boolean;
  createdAt: string;
}

export interface EstimationHistoryResponse {
  ticketId: string;
  estimations: EffortEstimation[];
  totalVersions: number;
}

export interface CreateEstimationRequest {
  complexity: ComplexityLevel;
  estimatedEffort: EffortSize;
  confidencePercent: number;
  estimationReasoning?: string;
  assumptions?: string;
  sessionId?: string;
}

export interface ReviseEstimationRequest {
  complexity: ComplexityLevel;
  estimatedEffort: EffortSize;
  confidencePercent: number;
  estimationReasoning?: string;
  assumptions?: string;
  revisionReason: string;
  sessionId?: string;
}

export interface RecordActualEffortRequest {
  actualEffort: EffortSize;
  varianceNotes?: string;
}
