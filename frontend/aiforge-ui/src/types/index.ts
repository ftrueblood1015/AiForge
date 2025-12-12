// Enums
export type TicketStatus = 'ToDo' | 'InProgress' | 'InReview' | 'Done';
export type TicketType = 'Task' | 'Bug' | 'Feature' | 'Enhancement';
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';
export type HandoffType = 'SessionEnd' | 'Blocker' | 'Milestone' | 'ContextDump';
export type ProgressOutcome = 'Success' | 'Failure' | 'Partial' | 'Blocked';

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
