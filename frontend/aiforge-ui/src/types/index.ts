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

// Code Intelligence Enums
export type FileChangeType = 'Created' | 'Modified' | 'Deleted' | 'Renamed';
export type TestOutcome = 'Passed' | 'Failed' | 'Skipped' | 'NotRun';
export type DebtCategory = 'Performance' | 'Security' | 'Maintainability' | 'Testing' | 'Documentation' | 'Architecture';
export type DebtSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type DebtStatus = 'Open' | 'InProgress' | 'Resolved' | 'Accepted';

// Code Intelligence Entities
export interface FileChange {
  id: string;
  ticketId: string;
  ticketKey: string | null;
  filePath: string;
  changeType: FileChangeType;
  oldFilePath: string | null;
  changeReason: string | null;
  linesAdded: number | null;
  linesRemoved: number | null;
  sessionId: string | null;
  createdAt: string;
}

export interface TestLink {
  id: string;
  ticketId: string;
  ticketKey: string | null;
  testFilePath: string;
  testName: string | null;
  testedFunctionality: string | null;
  outcome: TestOutcome | null;
  linkedFilePath: string | null;
  sessionId: string | null;
  createdAt: string;
  lastRunAt: string | null;
}

export interface TechnicalDebt {
  id: string;
  originatingTicketId: string;
  originatingTicketKey: string | null;
  resolutionTicketId: string | null;
  resolutionTicketKey: string | null;
  title: string;
  description: string | null;
  category: DebtCategory;
  severity: DebtSeverity;
  status: DebtStatus;
  rationale: string | null;
  affectedFiles: string | null;
  sessionId: string | null;
  createdAt: string;
  resolvedAt: string | null;
}

// Code Intelligence API Types
export interface FileHistoryResponse {
  filePath: string;
  changes: FileChange[];
  totalTickets: number;
}

export interface HotFile {
  filePath: string;
  changeCount: number;
  lastModified: string;
}

export interface FileCoverageResponse {
  filePath: string;
  tests: TestLink[];
  totalTests: number;
}

export interface DebtBacklogResponse {
  items: TechnicalDebt[];
  totalCount: number;
}

export interface DebtSummaryResponse {
  totalOpen: number;
  totalResolved: number;
  byCategory: Record<string, number>;
  bySeverity: Record<string, number>;
}

// Code Intelligence Request Types
export interface LogFileChangeRequest {
  filePath: string;
  changeType: FileChangeType;
  oldFilePath?: string;
  changeReason?: string;
  linesAdded?: number;
  linesRemoved?: number;
  sessionId?: string;
}

export interface LinkTestRequest {
  testFilePath: string;
  testName?: string;
  testedFunctionality?: string;
  linkedFilePath?: string;
  outcome?: TestOutcome;
  sessionId?: string;
}

export interface UpdateTestOutcomeRequest {
  outcome: TestOutcome;
}

export interface CreateDebtRequest {
  title: string;
  description?: string;
  category: DebtCategory;
  severity: DebtSeverity;
  rationale?: string;
  affectedFiles?: string;
  sessionId?: string;
}

export interface UpdateDebtRequest {
  title?: string;
  description?: string;
  category?: DebtCategory;
  severity?: DebtSeverity;
  status?: DebtStatus;
  rationale?: string;
  affectedFiles?: string;
}

export interface ResolveDebtRequest {
  resolutionTicketId?: string;
}

// Analytics Types
export interface AnalyticsDashboard {
  totalTickets: number;
  ticketsInProgress: number;
  ticketsCompleted: number;
  totalSessions: number;
  totalDecisions: number;
  overallAverageConfidence: number;
  lowConfidenceDecisionCount: number;
  recentLowConfidenceDecisions: LowConfidenceDecision[];
  topHotFiles: AnalyticsHotFile[];
  openTechnicalDebtCount: number;
  totalTokensUsed: number;
  totalMinutesWorked: number;
  handoffsCreated: number;
  recentActivity: RecentActivity[];
  generatedAt: string;
}

export interface LowConfidenceDecision {
  reasoningLogId: string;
  ticketId: string;
  ticketKey: string;
  ticketTitle: string;
  decisionPoint: string;
  chosenOption: string;
  rationale: string;
  confidencePercent: number;
  sessionId: string | null;
  createdAt: string;
}

export interface ConfidenceTrend {
  dataPoints: ConfidenceTrendPoint[];
  overallAverageConfidence: number;
  totalDecisions: number;
  lowConfidenceCount: number;
  startDate: string | null;
  endDate: string | null;
}

export interface ConfidenceTrendPoint {
  date: string;
  averageConfidence: number;
  decisionCount: number;
  lowConfidenceCount: number;
}

export interface TicketConfidenceSummary {
  ticketId: string;
  ticketKey: string;
  ticketTitle: string;
  ticketStatus: string;
  averageConfidence: number;
  totalDecisions: number;
  lowConfidenceDecisions: number;
  lowestConfidence: number | null;
  lastDecisionAt: string | null;
}

export interface AnalyticsHotFile {
  filePath: string;
  modificationCount: number;
  ticketCount: number;
  totalLinesAdded: number;
  totalLinesRemoved: number;
  firstModified: string | null;
  lastModified: string | null;
  recentTicketKeys: string[];
}

export interface FileCorrelation {
  filePath: string;
  correlatedFiles: CorrelatedFile[];
}

export interface CorrelatedFile {
  filePath: string;
  cooccurrenceCount: number;
  correlationStrength: number;
}

export interface RecurringIssue {
  pattern: string;
  occurrenceCount: number;
  relatedTickets: RelatedTicket[];
}

export interface RelatedTicket {
  ticketId: string;
  ticketKey: string;
  title: string;
  status: string;
  createdAt: string;
}

export interface DebtPatternSummary {
  totalDebtItems: number;
  openDebtItems: number;
  resolvedDebtItems: number;
  byCategory: DebtByCategory[];
  bySeverity: DebtBySeverity[];
  topHotspots: DebtHotspot[];
}

export interface DebtByCategory {
  category: string;
  count: number;
  openCount: number;
}

export interface DebtBySeverity {
  severity: string;
  count: number;
  openCount: number;
}

export interface DebtHotspot {
  filePath: string;
  debtItemCount: number;
  categories: string[];
}

export interface SessionMetrics {
  id: string;
  ticketId: string;
  ticketKey: string;
  sessionId: string;
  sessionStartedAt: string;
  sessionEndedAt: string | null;
  durationMinutes: number | null;
  inputTokens: number | null;
  outputTokens: number | null;
  totalTokens: number | null;
  decisionsLogged: number;
  progressEntriesLogged: number;
  filesModified: number;
  handoffCreated: boolean;
  notes: string | null;
  createdAt: string;
}

export interface TicketSessionAnalytics {
  ticketId: string;
  ticketKey: string;
  ticketTitle: string;
  totalSessions: number;
  totalDurationMinutes: number;
  averageDurationMinutes: number | null;
  totalInputTokens: number;
  totalOutputTokens: number;
  totalTokens: number;
  totalDecisions: number;
  totalProgressEntries: number;
  totalFilesModified: number;
  handoffsCreated: number;
  sessions: SessionMetrics[];
}

export interface ProductivityMetrics {
  totalTicketsCompleted: number;
  totalSessions: number;
  totalDurationMinutes: number;
  totalTokensUsed: number;
  averageSessionsPerTicket: number;
  averageMinutesPerTicket: number;
  averageTokensPerTicket: number;
  averageDecisionsPerTicket: number;
  byTicketType: ProductivityByType[];
  dailyTrend: ProductivityByDay[];
  startDate: string | null;
  endDate: string | null;
}

export interface ProductivityByType {
  ticketType: string;
  ticketCount: number;
  averageMinutes: number;
  averageTokens: number;
}

export interface ProductivityByDay {
  date: string;
  sessionCount: number;
  ticketsWorkedOn: number;
  totalMinutes: number;
  totalTokens: number;
}

export interface RecentActivity {
  activityType: string;
  description: string;
  ticketKey: string | null;
  timestamp: string;
}
