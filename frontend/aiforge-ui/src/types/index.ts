// Enums
export type TicketStatus = 'ToDo' | 'InProgress' | 'InReview' | 'Done';
export type TicketType = 'Task' | 'Bug' | 'Feature' | 'Enhancement';
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';
export type HandoffType = 'SessionEnd' | 'Blocker' | 'Milestone' | 'ContextDump';
export type ProgressOutcome = 'Success' | 'Failure' | 'Partial' | 'Blocked';
export type PlanStatus = 'Draft' | 'Approved' | 'Superseded' | 'Rejected';
export type ComplexityLevel = 'Low' | 'Medium' | 'High' | 'VeryHigh';
export type EffortSize = 'XSmall' | 'Small' | 'Medium' | 'Large' | 'XLarge';

// Agent & Skill Enums
export type AgentType = 'Claude' | 'GPT' | 'Gemini' | 'Custom';
export type AgentStatus = 'Idle' | 'Working' | 'Paused' | 'Disabled' | 'Error';
export type SkillCategory = 'Workflow' | 'Analysis' | 'Documentation' | 'Generation' | 'Testing' | 'Custom';
export type ConfigurationScope = 'Organization' | 'Project';

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
  projectKey: string;
  key: string;
  number: number;
  title: string;
  description: string | null;
  type: TicketType;
  status: TicketStatus;
  priority: Priority;
  parentTicketId: string | null;
  currentHandoffSummary: string | null;
  subTicketCount: number;
  createdAt: string;
  updatedAt: string;
  project?: Project;
  subTickets?: SubTicketSummary[];
  comments?: Comment[];
}

export interface SubTicketSummary {
  id: string;
  key: string;
  title: string;
  status: TicketStatus;
  type: TicketType;
  priority: Priority;
  createdAt: string;
  updatedAt: string;
}

export interface TicketDetail extends Ticket {
  subTickets: SubTicketSummary[];
  completedSubTicketCount: number;
  subTicketProgress: number;
  parentTicket: SubTicketSummary | null;
  commentCount: number;
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

export interface CreateSubTicketRequest {
  title: string;
  description?: string;
  type?: TicketType;
  priority?: Priority;
}

export interface MoveSubTicketRequest {
  newParentTicketId?: string | null;
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

// ==========================================
// Agent & Skill Configuration Types
// ==========================================

// Agent Entity
export interface Agent {
  id: string;
  agentKey: string;
  name: string;
  description: string | null;
  systemPrompt: string | null;
  instructions: string | null;
  agentType: AgentType;
  capabilities: string[];
  status: AgentStatus;
  organizationId: string | null;
  projectId: string | null;
  scope: ConfigurationScope;
  isEnabled: boolean;
  createdAt: string;
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface AgentListItem {
  id: string;
  agentKey: string;
  name: string;
  description: string | null;
  agentType: AgentType;
  status: AgentStatus;
  scope: ConfigurationScope;
  isEnabled: boolean;
}

// Skill Entity
export interface Skill {
  id: string;
  skillKey: string;
  name: string;
  description: string | null;
  content: string;
  category: SkillCategory;
  organizationId: string | null;
  projectId: string | null;
  scope: ConfigurationScope;
  isPublished: boolean;
  createdAt: string;
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface SkillListItem {
  id: string;
  skillKey: string;
  name: string;
  description: string | null;
  category: SkillCategory;
  scope: ConfigurationScope;
  isPublished: boolean;
}

// PromptTemplate Entity
export interface PromptTemplate {
  id: string;
  templateKey: string;
  name: string;
  description: string | null;
  template: string;
  variables: string[];
  category: string;
  organizationId: string | null;
  projectId: string | null;
  scope: ConfigurationScope;
  isPublished: boolean;
  createdAt: string;
  createdBy: string | null;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface PromptTemplateListItem {
  id: string;
  templateKey: string;
  name: string;
  description: string | null;
  category: string;
  variables: string[];
  scope: ConfigurationScope;
  isPublished: boolean;
}

// Agent API Request/Response Types
export interface CreateAgentRequest {
  agentKey: string;
  name: string;
  description?: string;
  systemPrompt?: string;
  instructions?: string;
  agentType?: AgentType;
  capabilities?: string[];
  organizationId?: string;
  projectId?: string;
}

export interface UpdateAgentRequest {
  name?: string;
  description?: string;
  systemPrompt?: string;
  instructions?: string;
  agentType?: AgentType;
  capabilities?: string[];
  status?: AgentStatus;
  isEnabled?: boolean;
}

export interface AgentListResponse {
  items: AgentListItem[];
  totalCount: number;
}

export interface AgentSearchParams {
  organizationId?: string;
  projectId?: string;
  agentType?: AgentType;
  status?: AgentStatus;
  isEnabled?: boolean;
}

// Skill API Request/Response Types
export interface CreateSkillRequest {
  skillKey: string;
  name: string;
  description?: string;
  content: string;
  category?: SkillCategory;
  organizationId?: string;
  projectId?: string;
}

export interface UpdateSkillRequest {
  name?: string;
  description?: string;
  content?: string;
  category?: SkillCategory;
  isPublished?: boolean;
}

export interface SkillListResponse {
  items: SkillListItem[];
  totalCount: number;
}

export interface SkillSearchParams {
  organizationId?: string;
  projectId?: string;
  category?: SkillCategory;
  isPublished?: boolean;
}

// PromptTemplate API Request/Response Types
export interface CreatePromptTemplateRequest {
  templateKey: string;
  name: string;
  description?: string;
  template: string;
  variables?: string[];
  category: string;
  organizationId?: string;
  projectId?: string;
}

export interface UpdatePromptTemplateRequest {
  name?: string;
  description?: string;
  template?: string;
  variables?: string[];
  category?: string;
  isPublished?: boolean;
}

export interface RenderTemplateRequest {
  variables: Record<string, string>;
}

export interface RenderTemplateResponse {
  renderedContent: string;
  missingVariables: string[];
}

export interface PromptTemplateListResponse {
  items: PromptTemplateListItem[];
  totalCount: number;
}

export interface PromptTemplateSearchParams {
  organizationId?: string;
  projectId?: string;
  category?: string;
}

// ==========================================
// Work Queue Types
// ==========================================

// Work Queue Enums
export type WorkQueueStatus = 'Active' | 'Paused' | 'Completed' | 'Archived';
export type WorkQueueItemStatus = 'Pending' | 'InProgress' | 'Completed' | 'Skipped' | 'Blocked';
export type WorkItemType = 'Task' | 'UserStory';

// Context Helper
export interface ContextHelper {
  currentFocus: string;
  keyDecisions: string[];
  blockersResolved: string[];
  nextSteps: string[];
  lastUpdated: string;
}

// Work Queue Entities
export interface WorkQueue {
  id: string;
  name: string;
  description: string | null;
  projectId: string;
  projectName: string;
  implementationPlanId: string | null;
  implementationPlanTitle: string | null;
  status: WorkQueueStatus;
  checkedOutBy: string | null;
  checkedOutAt: string | null;
  checkoutExpiresAt: string | null;
  itemCount: number;
  createdAt: string;
  createdBy: string | null;
}

export interface WorkQueueDetail extends WorkQueue {
  context: ContextHelper;
  items: WorkQueueItem[];
}

export interface WorkQueueItem {
  id: string;
  workItemId: string;
  workItemType: WorkItemType;
  workItemTitle: string;
  position: number;
  status: WorkQueueItemStatus;
  notes: string | null;
  addedAt: string;
  addedBy: string | null;
  completedAt: string | null;
}

// Tiered Context Response
export interface TieredContextResponse {
  tier: number;
  estimatedTokens: number;
  tier1: QueueContextTier1;
  tier2: QueueContextTier2 | null;
  tier3: QueueContextTier3 | null;
  tier4: QueueContextTier4 | null;
}

export interface QueueContextTier1 {
  queueName: string;
  currentItemTitle: string | null;
  totalItems: number;
  completedItems: number;
  context: ContextHelper;
  isStale: boolean;
  staleWarning: string | null;
}

export interface QueueContextTier2 {
  implementationPlanTitle: string | null;
  implementationPlanSummary: string | null;
  planOutline: string[];
}

export interface QueueContextTier3 {
  currentItem: WorkQueueItem | null;
  itemDescription: string | null;
  acceptanceCriteria: string[] | null;
  nextItems: WorkQueueItem[];
}

export interface QueueContextTier4 {
  recentFileSnapshots: FileSnapshotSummary[];
  relatedFiles: string[];
}

export interface FileSnapshotSummary {
  filePath: string;
  changeType: string | null;
  capturedAt: string;
}

// Work Queue Request DTOs
export interface CreateWorkQueueRequest {
  name: string;
  description?: string;
  implementationPlanId?: string;
}

export interface UpdateWorkQueueRequest {
  name?: string;
  description?: string;
  status?: WorkQueueStatus;
  implementationPlanId?: string;
}

export interface AddQueueItemRequest {
  workItemId: string;
  workItemType: WorkItemType;
  position?: number;
  notes?: string;
}

export interface UpdateQueueItemRequest {
  position?: number;
  status?: WorkQueueItemStatus;
  notes?: string;
}

export interface ReorderItemsRequest {
  itemIds: string[];
}

export interface CheckoutRequest {
  durationMinutes?: number;
}

export interface UpdateContextRequest {
  currentFocus?: string;
  appendKeyDecisions?: string[];
  appendBlockersResolved?: string[];
  replaceNextSteps?: string[];
}

export interface WorkQueueSearchParams {
  status?: WorkQueueStatus;
}

// ==========================================
// Skill Chain Types
// ==========================================

// Skill Chain Enums
export type TransitionType = 'NextLink' | 'GoToLink' | 'Complete' | 'Retry' | 'Escalate';
export type ChainExecutionStatus = 'Pending' | 'Running' | 'Paused' | 'Completed' | 'Failed' | 'Cancelled';
export type LinkExecutionOutcome = 'Pending' | 'Success' | 'Failure' | 'Skipped';

// Skill Chain Entity
export interface SkillChain {
  id: string;
  chainKey: string;
  name: string;
  description: string | null;
  inputSchema: string | null;
  maxTotalFailures: number;
  organizationId: string | null;
  projectId: string | null;
  scope: ConfigurationScope;
  isPublished: boolean;
  links: SkillChainLink[];
  createdAt: string;
  createdBy: string | null;
  updatedAt: string;
  updatedBy: string | null;
}

export interface SkillChainSummary {
  id: string;
  chainKey: string;
  name: string;
  description: string | null;
  scope: ConfigurationScope;
  isPublished: boolean;
  linkCount: number;
  executionCount: number;
  createdAt: string;
}

// Skill Chain Link Entity
export interface SkillChainLink {
  id: string;
  skillChainId: string;
  position: number;
  name: string;
  description: string | null;
  skillId: string;
  skillName: string | null;
  skillKey: string | null;
  agentId: string | null;
  agentName: string | null;
  agentKey: string | null;
  maxRetries: number;
  onSuccessTransition: TransitionType;
  onSuccessTargetLinkId: string | null;
  onFailureTransition: TransitionType;
  onFailureTargetLinkId: string | null;
  linkConfig: string | null;
}

// Skill Chain Execution Entity
export interface SkillChainExecution {
  id: string;
  skillChainId: string;
  chainKey: string | null;
  chainName: string | null;
  ticketId: string | null;
  ticketKey: string | null;
  status: ChainExecutionStatus;
  currentLinkId: string | null;
  currentLinkName: string | null;
  currentLinkPosition: number | null;
  inputValues: string | null;
  executionContext: string | null;
  totalFailureCount: number;
  requiresHumanIntervention: boolean;
  interventionReason: string | null;
  startedAt: string;
  completedAt: string | null;
  startedBy: string | null;
  completedBy: string | null;
  linkExecutions: SkillChainLinkExecution[];
}

export interface SkillChainExecutionSummary {
  id: string;
  skillChainId: string;
  chainName: string | null;
  ticketId: string | null;
  ticketKey: string | null;
  status: ChainExecutionStatus;
  currentLinkName: string | null;
  totalFailureCount: number;
  requiresHumanIntervention: boolean;
  startedAt: string;
  completedAt: string | null;
}

// Skill Chain Link Execution Entity
export interface SkillChainLinkExecution {
  id: string;
  skillChainExecutionId: string;
  skillChainLinkId: string;
  linkName: string | null;
  linkPosition: number | null;
  attemptNumber: number;
  outcome: LinkExecutionOutcome;
  input: string | null;
  output: string | null;
  errorDetails: string | null;
  transitionTaken: TransitionType | null;
  startedAt: string;
  completedAt: string | null;
  executedBy: string | null;
}

// Skill Chain API Request Types
export interface CreateSkillChainRequest {
  chainKey: string;
  name: string;
  description?: string;
  inputSchema?: string;
  maxTotalFailures?: number;
  organizationId?: string;
  projectId?: string;
}

export interface UpdateSkillChainRequest {
  name?: string;
  description?: string;
  inputSchema?: string;
  maxTotalFailures?: number;
  isPublished?: boolean;
}

export interface CreateSkillChainLinkRequest {
  name: string;
  description?: string;
  skillId: string;
  agentId?: string;
  maxRetries?: number;
  onSuccessTransition?: TransitionType;
  onSuccessTargetLinkId?: string;
  onFailureTransition?: TransitionType;
  onFailureTargetLinkId?: string;
  linkConfig?: string;
  position?: number;
}

export interface UpdateSkillChainLinkRequest {
  name?: string;
  description?: string;
  skillId?: string;
  agentId?: string;
  maxRetries?: number;
  onSuccessTransition?: TransitionType;
  onSuccessTargetLinkId?: string;
  onFailureTransition?: TransitionType;
  onFailureTargetLinkId?: string;
  linkConfig?: string;
}

export interface ReorderLinksRequest {
  linkIdsInOrder: string[];
}

// Skill Chain Execution API Request Types
export interface StartChainExecutionRequest {
  skillChainId: string;
  ticketId?: string;
  inputValues?: string;
  startedBy?: string;
}

export interface RecordLinkOutcomeRequest {
  linkId: string;
  outcome: 'Success' | 'Failure';
  output?: string;
  errorDetails?: string;
  executedBy?: string;
}

export interface ResumeExecutionRequest {
  resumedBy?: string;
  additionalContext?: string;
}

export interface ResolveInterventionRequest {
  resolution: string;
  nextAction: 'Retry' | 'GoToLink' | 'Complete' | 'Escalate';
  targetLinkId?: string;
  resolvedBy?: string;
}

export interface PauseExecutionRequest {
  reason: string;
}

export interface CancelExecutionRequest {
  reason: string;
}

// Skill Chain Search Params
export interface SkillChainSearchParams {
  organizationId?: string;
  projectId?: string;
  publishedOnly?: boolean;
}

export interface SkillChainExecutionSearchParams {
  chainId?: string;
  ticketId?: string;
  status?: ChainExecutionStatus;
}
