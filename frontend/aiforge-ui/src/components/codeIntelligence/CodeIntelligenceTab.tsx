import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Chip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Skeleton,
  Alert,
  Divider,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  InsertDriveFile as FileIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  DriveFileRenameOutline as RenameIcon,
  Science as TestIcon,
  CheckCircle as PassedIcon,
  Cancel as FailedIcon,
  RemoveCircle as SkippedIcon,
  HelpOutline as NotRunIcon,
  Warning as DebtIcon,
} from '@mui/icons-material';
import { fileChangeApi, testLinkApi, technicalDebtApi } from '../../api/codeIntelligence';
import type { FileChange, TestLink, TechnicalDebt, FileChangeType, TestOutcome, DebtSeverity } from '../../types';

interface CodeIntelligenceTabProps {
  ticketId: string;
}

const changeTypeIcons: Record<FileChangeType, React.ReactElement> = {
  Created: <AddIcon color="success" />,
  Modified: <EditIcon color="primary" />,
  Deleted: <DeleteIcon color="error" />,
  Renamed: <RenameIcon color="info" />,
};

const outcomeIcons: Record<TestOutcome, React.ReactElement> = {
  Passed: <PassedIcon color="success" />,
  Failed: <FailedIcon color="error" />,
  Skipped: <SkippedIcon color="warning" />,
  NotRun: <NotRunIcon color="disabled" />,
};

const severityColors: Record<DebtSeverity, 'error' | 'warning' | 'info' | 'success'> = {
  Critical: 'error',
  High: 'warning',
  Medium: 'info',
  Low: 'success',
};

export default function CodeIntelligenceTab({ ticketId }: CodeIntelligenceTabProps) {
  const [fileChanges, setFileChanges] = useState<FileChange[]>([]);
  const [testLinks, setTestLinks] = useState<TestLink[]>([]);
  const [debts, setDebts] = useState<TechnicalDebt[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, [ticketId]);

  const loadData = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [files, tests, debtItems] = await Promise.all([
        fileChangeApi.getByTicket(ticketId),
        testLinkApi.getByTicket(ticketId),
        technicalDebtApi.getByTicket(ticketId),
      ]);
      setFileChanges(files);
      setTestLinks(tests);
      setDebts(debtItems);
    } catch (err) {
      setError('Failed to load code intelligence data');
      console.error('Error loading code intelligence:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box>
        <Skeleton variant="rectangular" height={56} sx={{ mb: 1, borderRadius: 1 }} />
        <Skeleton variant="rectangular" height={56} sx={{ mb: 1, borderRadius: 1 }} />
        <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  const hasNoData = fileChanges.length === 0 && testLinks.length === 0 && debts.length === 0;

  if (hasNoData) {
    return (
      <Box sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
        <FileIcon sx={{ fontSize: 48, opacity: 0.5, mb: 1 }} />
        <Typography variant="body1">No code intelligence data yet</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          File changes, tests, and technical debt will appear here as work progresses.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* File Changes Section */}
      <Accordion defaultExpanded={fileChanges.length > 0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <FileIcon />
            <Typography fontWeight={500}>File Changes</Typography>
            <Chip label={fileChanges.length} size="small" />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          {fileChanges.length === 0 ? (
            <Typography color="text.secondary" variant="body2">
              No file changes recorded
            </Typography>
          ) : (
            <List dense disablePadding>
              {fileChanges.map((fc) => (
                <ListItem key={fc.id} sx={{ py: 0.5 }}>
                  <ListItemIcon sx={{ minWidth: 36 }}>
                    {changeTypeIcons[fc.changeType]}
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {fc.filePath}
                        </Typography>
                        {fc.changeType === 'Renamed' && fc.oldFilePath && (
                          <Typography variant="caption" color="text.secondary">
                            (from {fc.oldFilePath})
                          </Typography>
                        )}
                      </Box>
                    }
                    secondary={
                      <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                        {fc.changeReason && (
                          <Typography variant="caption">{fc.changeReason}</Typography>
                        )}
                        {(fc.linesAdded !== null || fc.linesRemoved !== null) && (
                          <Typography variant="caption" color="text.secondary">
                            {fc.linesAdded !== null && <span style={{ color: 'green' }}>+{fc.linesAdded}</span>}
                            {fc.linesAdded !== null && fc.linesRemoved !== null && ' / '}
                            {fc.linesRemoved !== null && <span style={{ color: 'red' }}>-{fc.linesRemoved}</span>}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </AccordionDetails>
      </Accordion>

      {/* Tests Section */}
      <Accordion defaultExpanded={testLinks.length > 0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <TestIcon />
            <Typography fontWeight={500}>Tests</Typography>
            <Chip label={testLinks.length} size="small" />
            {testLinks.length > 0 && (
              <Box sx={{ display: 'flex', gap: 0.5, ml: 1 }}>
                {testLinks.filter(t => t.outcome === 'Passed').length > 0 && (
                  <Chip
                    icon={<PassedIcon />}
                    label={testLinks.filter(t => t.outcome === 'Passed').length}
                    size="small"
                    color="success"
                    variant="outlined"
                  />
                )}
                {testLinks.filter(t => t.outcome === 'Failed').length > 0 && (
                  <Chip
                    icon={<FailedIcon />}
                    label={testLinks.filter(t => t.outcome === 'Failed').length}
                    size="small"
                    color="error"
                    variant="outlined"
                  />
                )}
              </Box>
            )}
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          {testLinks.length === 0 ? (
            <Typography color="text.secondary" variant="body2">
              No tests linked
            </Typography>
          ) : (
            <List dense disablePadding>
              {testLinks.map((tl) => (
                <ListItem key={tl.id} sx={{ py: 0.5 }}>
                  <ListItemIcon sx={{ minWidth: 36 }}>
                    {tl.outcome ? outcomeIcons[tl.outcome] : <NotRunIcon color="disabled" />}
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {tl.testFilePath}
                        </Typography>
                        {tl.testName && (
                          <Chip label={tl.testName} size="small" variant="outlined" />
                        )}
                      </Box>
                    }
                    secondary={
                      <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                        {tl.testedFunctionality && (
                          <Typography variant="caption">{tl.testedFunctionality}</Typography>
                        )}
                        {tl.linkedFilePath && (
                          <Typography variant="caption" color="text.secondary">
                            Tests: {tl.linkedFilePath}
                          </Typography>
                        )}
                        {tl.lastRunAt && (
                          <Typography variant="caption" color="text.secondary">
                            Last run: {new Date(tl.lastRunAt).toLocaleString()}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </AccordionDetails>
      </Accordion>

      {/* Technical Debt Section */}
      <Accordion defaultExpanded={debts.length > 0}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <DebtIcon />
            <Typography fontWeight={500}>Technical Debt</Typography>
            <Chip label={debts.length} size="small" color={debts.length > 0 ? 'warning' : 'default'} />
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          {debts.length === 0 ? (
            <Typography color="text.secondary" variant="body2">
              No technical debt flagged
            </Typography>
          ) : (
            <List dense disablePadding>
              {debts.map((debt, index) => (
                <Box key={debt.id}>
                  {index > 0 && <Divider sx={{ my: 1 }} />}
                  <ListItem sx={{ py: 0.5, flexDirection: 'column', alignItems: 'flex-start' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                      <Typography variant="body2" fontWeight={500}>
                        {debt.title}
                      </Typography>
                      <Chip
                        label={debt.severity}
                        size="small"
                        color={severityColors[debt.severity]}
                      />
                      <Chip label={debt.category} size="small" variant="outlined" />
                      <Chip
                        label={debt.status}
                        size="small"
                        variant={debt.status === 'Resolved' ? 'filled' : 'outlined'}
                        color={debt.status === 'Resolved' ? 'success' : 'default'}
                      />
                    </Box>
                    {debt.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        {debt.description}
                      </Typography>
                    )}
                    {debt.rationale && (
                      <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, fontStyle: 'italic' }}>
                        Why: {debt.rationale}
                      </Typography>
                    )}
                    {debt.affectedFiles && (
                      <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                        Files: {debt.affectedFiles}
                      </Typography>
                    )}
                  </ListItem>
                </Box>
              ))}
            </List>
          )}
        </AccordionDetails>
      </Accordion>
    </Box>
  );
}
