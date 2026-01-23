import {
  Box,
  Typography,
  Card,
  CardContent,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Divider,
  Alert,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  LightbulbOutlined as FocusIcon,
  Gavel as DecisionIcon,
  CheckCircleOutline as ResolvedIcon,
  ArrowForward as NextStepIcon,
  Description as PlanIcon,
  Assignment as ItemIcon,
  Code as FileIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import type { TieredContextResponse, WorkQueueItem } from '../../types';

interface TierContentProps {
  data: TieredContextResponse;
  selectedTier: number;
}

export default function TierContent({ data, selectedTier }: TierContentProps) {
  const { tier1, tier2, tier3, tier4 } = data;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {/* Tier 1: Core Context */}
      <Card variant="outlined">
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <Chip label="Tier 1" size="small" color="primary" />
            <Typography variant="subtitle1">Core Context</Typography>
          </Box>

          {/* Staleness Warning */}
          {tier1.isStale && tier1.staleWarning && (
            <Alert severity="warning" icon={<WarningIcon />} sx={{ mb: 2 }}>
              {tier1.staleWarning}
            </Alert>
          )}

          {/* Queue Overview */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="h6">{tier1.queueName}</Typography>
            <Typography variant="body2" color="text.secondary">
              {tier1.completedItems} / {tier1.totalItems} items completed
            </Typography>
          </Box>

          {/* Current Focus */}
          <Box
            sx={{
              p: 2,
              backgroundColor: 'primary.main',
              color: 'primary.contrastText',
              borderRadius: 1,
              mb: 2,
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <FocusIcon />
              <Typography variant="subtitle2">Current Focus</Typography>
            </Box>
            <Typography variant="body1">
              {tier1.context.currentFocus || 'No focus set'}
            </Typography>
          </Box>

          {/* Current Item */}
          {tier1.currentItemTitle && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Current Item
              </Typography>
              <Typography variant="body1">{tier1.currentItemTitle}</Typography>
            </Box>
          )}

          {/* Context Helper Sections */}
          <Accordion defaultExpanded={tier1.context.keyDecisions.length > 0}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <DecisionIcon color="info" fontSize="small" />
                <Typography variant="body2">Key Decisions</Typography>
                <Chip label={tier1.context.keyDecisions.length} size="small" />
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              {tier1.context.keyDecisions.length === 0 ? (
                <Typography color="text.secondary" variant="body2">
                  No decisions recorded.
                </Typography>
              ) : (
                <List dense disablePadding>
                  {tier1.context.keyDecisions.map((d, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemIcon sx={{ minWidth: 28 }}>
                        <DecisionIcon fontSize="small" color="info" />
                      </ListItemIcon>
                      <ListItemText primary={d} primaryTypographyProps={{ variant: 'body2' }} />
                    </ListItem>
                  ))}
                </List>
              )}
            </AccordionDetails>
          </Accordion>

          <Accordion defaultExpanded={tier1.context.blockersResolved.length > 0}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <ResolvedIcon color="success" fontSize="small" />
                <Typography variant="body2">Blockers Resolved</Typography>
                <Chip label={tier1.context.blockersResolved.length} size="small" />
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              {tier1.context.blockersResolved.length === 0 ? (
                <Typography color="text.secondary" variant="body2">
                  No blockers resolved.
                </Typography>
              ) : (
                <List dense disablePadding>
                  {tier1.context.blockersResolved.map((b, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemIcon sx={{ minWidth: 28 }}>
                        <ResolvedIcon fontSize="small" color="success" />
                      </ListItemIcon>
                      <ListItemText primary={b} primaryTypographyProps={{ variant: 'body2' }} />
                    </ListItem>
                  ))}
                </List>
              )}
            </AccordionDetails>
          </Accordion>

          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <NextStepIcon color="warning" fontSize="small" />
                <Typography variant="body2">Next Steps</Typography>
                <Chip label={tier1.context.nextSteps.length} size="small" />
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              {tier1.context.nextSteps.length === 0 ? (
                <Typography color="text.secondary" variant="body2">
                  No next steps defined.
                </Typography>
              ) : (
                <List dense disablePadding>
                  {tier1.context.nextSteps.map((s, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemIcon sx={{ minWidth: 28 }}>
                        <Typography variant="body2" color="text.secondary">
                          {i + 1}.
                        </Typography>
                      </ListItemIcon>
                      <ListItemText primary={s} primaryTypographyProps={{ variant: 'body2' }} />
                    </ListItem>
                  ))}
                </List>
              )}
            </AccordionDetails>
          </Accordion>
        </CardContent>
      </Card>

      {/* Tier 2: Implementation Plan */}
      {selectedTier >= 2 && tier2 && (
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Chip label="Tier 2" size="small" color="secondary" />
              <Typography variant="subtitle1">Implementation Plan</Typography>
            </Box>

            {tier2.implementationPlanTitle ? (
              <>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <PlanIcon color="action" />
                  <Typography variant="h6">{tier2.implementationPlanTitle}</Typography>
                </Box>

                {tier2.implementationPlanSummary && (
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {tier2.implementationPlanSummary}
                  </Typography>
                )}

                {tier2.planOutline.length > 0 && (
                  <Accordion defaultExpanded>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Typography variant="body2">Plan Outline</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                      <List dense disablePadding>
                        {tier2.planOutline.map((item, i) => (
                          <ListItem key={i} disableGutters>
                            <ListItemIcon sx={{ minWidth: 28 }}>
                              <Typography variant="body2" color="text.secondary">
                                {i + 1}.
                              </Typography>
                            </ListItemIcon>
                            <ListItemText
                              primary={item}
                              primaryTypographyProps={{ variant: 'body2' }}
                            />
                          </ListItem>
                        ))}
                      </List>
                    </AccordionDetails>
                  </Accordion>
                )}
              </>
            ) : (
              <Typography color="text.secondary" variant="body2">
                No implementation plan linked to this queue.
              </Typography>
            )}
          </CardContent>
        </Card>
      )}

      {/* Tier 3: Item Details */}
      {selectedTier >= 3 && tier3 && (
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Chip label="Tier 3" size="small" color="warning" />
              <Typography variant="subtitle1">Item Details</Typography>
            </Box>

            {/* Current Item Details */}
            {tier3.currentItem ? (
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  Current Item
                </Typography>
                <Card variant="outlined" sx={{ p: 2, backgroundColor: 'action.hover' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <ItemIcon color="primary" />
                    <Typography variant="body1" fontWeight="medium">
                      {tier3.currentItem.workItemTitle}
                    </Typography>
                    <Chip
                      label={tier3.currentItem.status}
                      size="small"
                      color={tier3.currentItem.status === 'Completed' ? 'success' : 'default'}
                    />
                  </Box>

                  {tier3.itemDescription && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                      {tier3.itemDescription}
                    </Typography>
                  )}

                  {tier3.acceptanceCriteria && tier3.acceptanceCriteria.length > 0 && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        Acceptance Criteria
                      </Typography>
                      <List dense disablePadding>
                        {tier3.acceptanceCriteria.map((ac, i) => (
                          <ListItem key={i} disableGutters>
                            <ListItemText
                              primary={`• ${ac}`}
                              primaryTypographyProps={{ variant: 'body2' }}
                            />
                          </ListItem>
                        ))}
                      </List>
                    </Box>
                  )}
                </Card>
              </Box>
            ) : (
              <Typography color="text.secondary" variant="body2" sx={{ mb: 2 }}>
                No current item selected.
              </Typography>
            )}

            {/* Next Items */}
            {tier3.nextItems.length > 0 && (
              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="body2">Next Items ({tier3.nextItems.length})</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <List dense disablePadding>
                    {tier3.nextItems.map((item, i) => (
                      <ListItem key={item.id} disableGutters>
                        <ListItemIcon sx={{ minWidth: 28 }}>
                          <Typography variant="body2" color="text.secondary">
                            {i + 1}.
                          </Typography>
                        </ListItemIcon>
                        <ListItemText
                          primary={item.workItemTitle}
                          secondary={item.notes}
                          primaryTypographyProps={{ variant: 'body2' }}
                        />
                        <Chip label={item.status} size="small" sx={{ ml: 1 }} />
                      </ListItem>
                    ))}
                  </List>
                </AccordionDetails>
              </Accordion>
            )}
          </CardContent>
        </Card>
      )}

      {/* Tier 4: File Snapshots */}
      {selectedTier >= 4 && tier4 && (
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Chip label="Tier 4" size="small" color="error" />
              <Typography variant="subtitle1">File Context</Typography>
            </Box>

            {/* Recent File Snapshots */}
            {tier4.recentFileSnapshots.length > 0 && (
              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <FileIcon fontSize="small" />
                    <Typography variant="body2">
                      Recent File Snapshots ({tier4.recentFileSnapshots.length})
                    </Typography>
                  </Box>
                </AccordionSummary>
                <AccordionDetails>
                  <List dense disablePadding>
                    {tier4.recentFileSnapshots.map((snapshot, i) => (
                      <ListItem key={i} disableGutters>
                        <ListItemIcon sx={{ minWidth: 28 }}>
                          <FileIcon fontSize="small" color="action" />
                        </ListItemIcon>
                        <ListItemText
                          primary={snapshot.filePath}
                          secondary={`${snapshot.changeType || 'Modified'} - ${new Date(
                            snapshot.capturedAt
                          ).toLocaleString()}`}
                          primaryTypographyProps={{ variant: 'body2', fontFamily: 'monospace' }}
                        />
                      </ListItem>
                    ))}
                  </List>
                </AccordionDetails>
              </Accordion>
            )}

            {/* Related Files */}
            {tier4.relatedFiles.length > 0 && (
              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="body2">
                    Related Files ({tier4.relatedFiles.length})
                  </Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <List dense disablePadding>
                    {tier4.relatedFiles.map((file, i) => (
                      <ListItem key={i} disableGutters>
                        <ListItemIcon sx={{ minWidth: 28 }}>
                          <FileIcon fontSize="small" color="action" />
                        </ListItemIcon>
                        <ListItemText
                          primary={file}
                          primaryTypographyProps={{ variant: 'body2', fontFamily: 'monospace' }}
                        />
                      </ListItem>
                    ))}
                  </List>
                </AccordionDetails>
              </Accordion>
            )}

            {tier4.recentFileSnapshots.length === 0 && tier4.relatedFiles.length === 0 && (
              <Typography color="text.secondary" variant="body2">
                No file context available.
              </Typography>
            )}
          </CardContent>
        </Card>
      )}
    </Box>
  );
}
