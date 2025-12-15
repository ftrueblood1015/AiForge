import { useState } from 'react';
import {
  Box,
  Typography,
  Chip,
  Card,
  CardContent,
  Grid,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Collapse,
  IconButton,
  Paper,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Description as HandoffIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  CheckCircle as CheckIcon,
  Warning as WarningIcon,
  Help as QuestionIcon,
  Block as BlockerIcon,
  InsertDriveFile as FileIcon,
  ArrowForward as NextStepIcon,
  Psychology as DecisionIcon,
  Lightbulb as AssumptionIcon,
  Science as TestIcon,
} from '@mui/icons-material';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { useTheme } from '@mui/material/styles';
import { useMarkdownStyles } from '../../hooks';
import type { HandoffDocument, HandoffType, FileSnapshot } from '../../types';
import CodeSnippet from './CodeSnippet';
import FileDiff from './FileDiff';

interface HandoffViewerProps {
  handoff: HandoffDocument;
  snapshots?: FileSnapshot[];
}

const typeConfig: Record<HandoffType, { color: 'default' | 'primary' | 'warning' | 'info'; label: string }> = {
  SessionEnd: { color: 'default', label: 'Session End' },
  Blocker: { color: 'warning', label: 'Blocker' },
  Milestone: { color: 'primary', label: 'Milestone' },
  ContextDump: { color: 'info', label: 'Context Dump' },
};

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div hidden={value !== index}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

export default function HandoffViewer({ handoff, snapshots = [] }: HandoffViewerProps) {
  const theme = useTheme();
  const markdownStyles = useMarkdownStyles();
  const [activeTab, setActiveTab] = useState(0);
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    assumptions: true,
    decisions: true,
    openQuestions: true,
    blockers: true,
    filesModified: false,
    testsAdded: false,
    nextSteps: true,
    warnings: true,
  });

  const config = typeConfig[handoff.type];
  const context = handoff.structuredContext;

  const toggleSection = (section: string) => {
    setExpandedSections((prev) => ({ ...prev, [section]: !prev[section] }));
  };

  const renderContextSection = (
    title: string,
    items: string[] | undefined,
    icon: React.ReactNode,
    sectionKey: string,
    color: 'primary' | 'secondary' | 'warning' | 'error' | 'info' | 'success' = 'primary'
  ) => {
    if (!items || items.length === 0) return null;

    return (
      <Box sx={{ mb: 2 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            cursor: 'pointer',
            '&:hover': { bgcolor: 'action.hover' },
            borderRadius: 1,
            p: 0.5,
          }}
          onClick={() => toggleSection(sectionKey)}
        >
          <Box sx={{ color: `${color}.main`, mr: 1, display: 'flex' }}>{icon}</Box>
          <Typography variant="subtitle2" sx={{ flex: 1 }}>
            {title} ({items.length})
          </Typography>
          <IconButton size="small">
            {expandedSections[sectionKey] ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
        <Collapse in={expandedSections[sectionKey]}>
          <List dense disablePadding sx={{ pl: 2 }}>
            {items.map((item, index) => (
              <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                <ListItemIcon sx={{ minWidth: 24 }}>
                  <CheckIcon fontSize="small" color={color} />
                </ListItemIcon>
                <ListItemText
                  primary={item}
                  primaryTypographyProps={{ variant: 'body2' }}
                />
              </ListItem>
            ))}
          </List>
        </Collapse>
      </Box>
    );
  };

  const renderDecisions = (decisions: { decision: string; rationale: string }[] | undefined) => {
    if (!decisions || decisions.length === 0) return null;

    return (
      <Box sx={{ mb: 2 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            cursor: 'pointer',
            '&:hover': { bgcolor: 'action.hover' },
            borderRadius: 1,
            p: 0.5,
          }}
          onClick={() => toggleSection('decisions')}
        >
          <Box sx={{ color: 'secondary.main', mr: 1, display: 'flex' }}>
            <DecisionIcon />
          </Box>
          <Typography variant="subtitle2" sx={{ flex: 1 }}>
            Decisions Made ({decisions.length})
          </Typography>
          <IconButton size="small">
            {expandedSections.decisions ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
        <Collapse in={expandedSections.decisions}>
          <Box sx={{ pl: 2 }}>
            {decisions.map((d, index) => (
              <Paper key={index} variant="outlined" sx={{ p: 1.5, mb: 1 }}>
                <Typography variant="body2" fontWeight={500}>
                  {d.decision}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  {d.rationale}
                </Typography>
              </Paper>
            ))}
          </Box>
        </Collapse>
      </Box>
    );
  };

  return (
    <Box>
      {/* Header */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
            <HandoffIcon color="primary" sx={{ fontSize: 40 }} />
            <Box sx={{ flex: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                <Typography variant="h5">{handoff.title}</Typography>
                <Chip label={config.label} color={config.color} size="small" />
                {!handoff.isActive && (
                  <Chip label="Superseded" color="default" size="small" variant="outlined" />
                )}
              </Box>
              <Typography variant="body2" color="text.secondary">
                {new Date(handoff.createdAt).toLocaleString()}
                {handoff.sessionId && ` • Session: ${handoff.sessionId.slice(0, 8)}...`}
              </Typography>
            </Box>
          </Box>

          {/* Summary */}
          <Box sx={{ mt: 2, p: 2, bgcolor: theme.palette.mode === 'dark' ? 'grey.800' : 'grey.100', borderRadius: 1 }}>
            <Typography variant="body1">{handoff.summary}</Typography>
          </Box>
        </CardContent>
      </Card>

      {/* Tabs */}
      <Tabs
        value={activeTab}
        onChange={(_, value) => setActiveTab(value)}
        sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
      >
        <Tab label="Content" />
        <Tab label="Structured Context" disabled={!context} />
        <Tab label={`File Changes (${snapshots.length})`} disabled={snapshots.length === 0} />
      </Tabs>

      {/* Content Tab */}
      <TabPanel value={activeTab} index={0}>
        <Card>
          <CardContent>
            <Box sx={markdownStyles}>
              <ReactMarkdown remarkPlugins={[remarkGfm]}>{handoff.content}</ReactMarkdown>
            </Box>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Structured Context Tab */}
      <TabPanel value={activeTab} index={1}>
        {context && (
          <Grid container spacing={3}>
            {/* Main Context */}
            <Grid size={{ xs: 12, md: 8 }}>
              <Card>
                <CardContent>
                  {renderContextSection(
                    'Assumptions',
                    context.assumptions,
                    <AssumptionIcon />,
                    'assumptions',
                    'info'
                  )}
                  {renderDecisions(context.decisionsMade)}
                  {renderContextSection(
                    'Next Steps',
                    context.nextSteps,
                    <NextStepIcon />,
                    'nextSteps',
                    'primary'
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Side Panel */}
            <Grid size={{ xs: 12, md: 4 }}>
              {/* Warnings */}
              {context.warnings && context.warnings.length > 0 && (
                <Card sx={{ mb: 2, borderColor: 'warning.main', borderWidth: 2 }}>
                  <CardContent>
                    {renderContextSection(
                      'Warnings',
                      context.warnings,
                      <WarningIcon />,
                      'warnings',
                      'warning'
                    )}
                  </CardContent>
                </Card>
              )}

              {/* Blockers */}
              {context.blockers && context.blockers.length > 0 && (
                <Card sx={{ mb: 2, borderColor: 'error.main', borderWidth: 2 }}>
                  <CardContent>
                    {renderContextSection(
                      'Blockers',
                      context.blockers,
                      <BlockerIcon />,
                      'blockers',
                      'error'
                    )}
                  </CardContent>
                </Card>
              )}

              {/* Open Questions */}
              {context.openQuestions && context.openQuestions.length > 0 && (
                <Card sx={{ mb: 2 }}>
                  <CardContent>
                    {renderContextSection(
                      'Open Questions',
                      context.openQuestions,
                      <QuestionIcon />,
                      'openQuestions',
                      'secondary'
                    )}
                  </CardContent>
                </Card>
              )}

              {/* Files Modified */}
              {context.filesModified && context.filesModified.length > 0 && (
                <Card sx={{ mb: 2 }}>
                  <CardContent>
                    {renderContextSection(
                      'Files Modified',
                      context.filesModified,
                      <FileIcon />,
                      'filesModified',
                      'info'
                    )}
                  </CardContent>
                </Card>
              )}

              {/* Tests Added */}
              {context.testsAdded && context.testsAdded.length > 0 && (
                <Card sx={{ mb: 2 }}>
                  <CardContent>
                    {renderContextSection(
                      'Tests Added',
                      context.testsAdded,
                      <TestIcon />,
                      'testsAdded',
                      'success'
                    )}
                  </CardContent>
                </Card>
              )}
            </Grid>
          </Grid>
        )}
      </TabPanel>

      {/* File Changes Tab */}
      <TabPanel value={activeTab} index={2}>
        {snapshots.length === 0 ? (
          <Typography color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            No file snapshots available
          </Typography>
        ) : (
          snapshots.map((snapshot) => (
            <Card key={snapshot.id} sx={{ mb: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <FileIcon color="action" />
                  <Typography variant="subtitle1" fontFamily="monospace">
                    {snapshot.filePath}
                  </Typography>
                  <Chip label={snapshot.language} size="small" variant="outlined" />
                </Box>
                <Divider sx={{ mb: 2 }} />
                {snapshot.contentBefore && snapshot.contentAfter ? (
                  <FileDiff
                    before={snapshot.contentBefore}
                    after={snapshot.contentAfter}
                    language={snapshot.language}
                  />
                ) : snapshot.contentAfter ? (
                  <Box>
                    <Chip label="New File" color="success" size="small" sx={{ mb: 1 }} />
                    <CodeSnippet code={snapshot.contentAfter} language={snapshot.language} />
                  </Box>
                ) : snapshot.contentBefore ? (
                  <Box>
                    <Chip label="Deleted File" color="error" size="small" sx={{ mb: 1 }} />
                    <CodeSnippet code={snapshot.contentBefore} language={snapshot.language} />
                  </Box>
                ) : (
                  <Typography color="text.secondary">No content available</Typography>
                )}
              </CardContent>
            </Card>
          ))
        )}
      </TabPanel>
    </Box>
  );
}
