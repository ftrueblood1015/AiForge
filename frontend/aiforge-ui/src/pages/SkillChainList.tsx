import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Tabs,
  Tab,
  Skeleton,
} from '@mui/material';
import Grid from '@mui/material/Grid';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Publish as PublishIcon,
  Unpublished as UnpublishIcon,
  PlayArrow as StartIcon,
  Visibility as ViewIcon,
  Warning as InterventionIcon,
} from '@mui/icons-material';
import { skillChainsApi, skillChainExecutionsApi } from '../api/skillChains';
import {
  ChainCard,
  ChainFilters,
  ExecutionCard,
  ExecutionFilters,
} from '../components/skillChains';
import type {
  SkillChainSummary,
  SkillChainExecutionSummary,
  SkillChainSearchParams,
  SkillChainExecutionSearchParams,
} from '../types';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

export default function SkillChainList() {
  const [tabValue, setTabValue] = useState(0);

  // Chains state
  const [chains, setChains] = useState<SkillChainSummary[]>([]);
  const [chainsLoading, setChainsLoading] = useState(true);
  const [chainsError, setChainsError] = useState<string | null>(null);
  const [chainFilters, setChainFilters] = useState<SkillChainSearchParams>({});

  // Executions state
  const [executions, setExecutions] = useState<SkillChainExecutionSummary[]>([]);
  const [executionsLoading, setExecutionsLoading] = useState(true);
  const [executionsError, setExecutionsError] = useState<string | null>(null);
  const [executionFilters, setExecutionFilters] = useState<SkillChainExecutionSearchParams>({});

  // Interventions state
  const [interventions, setInterventions] = useState<SkillChainExecutionSummary[]>([]);
  const [interventionsLoading, setInterventionsLoading] = useState(true);

  // Menu state
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [menuChain, setMenuChain] = useState<SkillChainSummary | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [chainToDelete, setChainToDelete] = useState<SkillChainSummary | null>(null);

  // Fetch chains
  useEffect(() => {
    const fetchChains = async () => {
      setChainsLoading(true);
      setChainsError(null);
      try {
        const data = await skillChainsApi.getAll(chainFilters);
        setChains(data);
      } catch (err) {
        setChainsError(err instanceof Error ? err.message : 'Failed to load chains');
      } finally {
        setChainsLoading(false);
      }
    };
    fetchChains();
  }, [chainFilters]);

  // Fetch executions
  useEffect(() => {
    const fetchExecutions = async () => {
      setExecutionsLoading(true);
      setExecutionsError(null);
      try {
        const data = await skillChainExecutionsApi.getAll(executionFilters);
        setExecutions(data);
      } catch (err) {
        setExecutionsError(err instanceof Error ? err.message : 'Failed to load executions');
      } finally {
        setExecutionsLoading(false);
      }
    };
    fetchExecutions();
  }, [executionFilters]);

  // Fetch interventions
  useEffect(() => {
    const fetchInterventions = async () => {
      setInterventionsLoading(true);
      try {
        const data = await skillChainExecutionsApi.getPendingInterventions();
        setInterventions(data);
      } catch {
        // Silent fail for interventions
      } finally {
        setInterventionsLoading(false);
      }
    };
    fetchInterventions();
  }, []);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleMenuOpen = (event: React.MouseEvent, chain: SkillChainSummary) => {
    setMenuAnchor(event.currentTarget as HTMLElement);
    setMenuChain(chain);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setMenuChain(null);
  };

  const handlePublishToggle = async () => {
    if (menuChain) {
      try {
        if (menuChain.isPublished) {
          await skillChainsApi.unpublish(menuChain.id);
        } else {
          await skillChainsApi.publish(menuChain.id);
        }
        // Refresh chains
        const data = await skillChainsApi.getAll(chainFilters);
        setChains(data);
      } catch (err) {
        setChainsError(err instanceof Error ? err.message : 'Failed to update chain');
      }
    }
    handleMenuClose();
  };

  const handleDeleteClick = () => {
    if (menuChain) {
      setChainToDelete(menuChain);
      setDeleteConfirmOpen(true);
    }
    handleMenuClose();
  };

  const handleDeleteConfirm = async () => {
    if (chainToDelete) {
      try {
        await skillChainsApi.delete(chainToDelete.id);
        setChains(chains.filter(c => c.id !== chainToDelete.id));
        setDeleteConfirmOpen(false);
        setChainToDelete(null);
      } catch (err) {
        setChainsError(err instanceof Error ? err.message : 'Failed to delete chain');
      }
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmOpen(false);
    setChainToDelete(null);
  };

  const renderChainsSkeleton = () => (
    <Grid container spacing={2}>
      {[1, 2, 3, 4, 5, 6].map((i) => (
        <Grid size={{ xs: 12, sm: 6, md: 4 }} key={i}>
          <Skeleton variant="rounded" height={180} />
        </Grid>
      ))}
    </Grid>
  );

  const renderExecutionsSkeleton = () => (
    <Grid container spacing={2}>
      {[1, 2, 3, 4].map((i) => (
        <Grid size={{ xs: 12, sm: 6 }} key={i}>
          <Skeleton variant="rounded" height={160} />
        </Grid>
      ))}
    </Grid>
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Skill Chains</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => {/* TODO: Open create dialog */}}
        >
          Create Chain
        </Button>
      </Box>

      {/* Interventions Alert */}
      {!interventionsLoading && interventions.length > 0 && (
        <Alert
          severity="warning"
          icon={<InterventionIcon />}
          sx={{ mb: 2 }}
          action={
            <Button
              color="inherit"
              size="small"
              onClick={() => setTabValue(2)}
            >
              View
            </Button>
          }
        >
          {interventions.length} execution{interventions.length > 1 ? 's' : ''} require{interventions.length === 1 ? 's' : ''} human intervention
        </Alert>
      )}

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={handleTabChange}>
          <Tab label={`Chains (${chains.length})`} />
          <Tab label={`Executions (${executions.length})`} />
          <Tab
            label={`Interventions (${interventions.length})`}
            sx={{
              color: interventions.length > 0 ? 'warning.main' : 'text.secondary',
            }}
          />
        </Tabs>
      </Box>

      {/* Chains Tab */}
      <TabPanel value={tabValue} index={0}>
        {chainsError && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setChainsError(null)}>
            {chainsError}
          </Alert>
        )}

        <ChainFilters
          filters={chainFilters}
          onChange={setChainFilters}
        />

        <Box sx={{ mt: 2 }}>
          {chainsLoading ? (
            renderChainsSkeleton()
          ) : chains.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 6, bgcolor: 'background.paper', borderRadius: 1 }}>
              <Typography color="text.secondary" gutterBottom>
                No skill chains configured
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Create your first skill chain to define orchestrated workflows.
              </Typography>
              <Button variant="contained" startIcon={<AddIcon />}>
                Create Chain
              </Button>
            </Box>
          ) : (
            <Grid container spacing={2}>
              {chains.map((chain) => (
                <Grid size={{ xs: 12, sm: 6, md: 4 }} key={chain.id}>
                  <ChainCard
                    chain={chain}
                    onMenuClick={handleMenuOpen}
                  />
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      </TabPanel>

      {/* Executions Tab */}
      <TabPanel value={tabValue} index={1}>
        {executionsError && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setExecutionsError(null)}>
            {executionsError}
          </Alert>
        )}

        <ExecutionFilters
          filters={executionFilters}
          onChange={setExecutionFilters}
        />

        <Box sx={{ mt: 2 }}>
          {executionsLoading ? (
            renderExecutionsSkeleton()
          ) : executions.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 6, bgcolor: 'background.paper', borderRadius: 1 }}>
              <Typography color="text.secondary" gutterBottom>
                No executions found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Start a chain execution to see it here.
              </Typography>
            </Box>
          ) : (
            <Grid container spacing={2}>
              {executions.map((execution) => (
                <Grid size={{ xs: 12, sm: 6 }} key={execution.id}>
                  <ExecutionCard
                    execution={execution}
                    onClick={() => {/* TODO: Navigate to detail */}}
                  />
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      </TabPanel>

      {/* Interventions Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box sx={{ mt: 2 }}>
          {interventionsLoading ? (
            renderExecutionsSkeleton()
          ) : interventions.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 6, bgcolor: 'background.paper', borderRadius: 1 }}>
              <Typography color="text.secondary" gutterBottom>
                No interventions required
              </Typography>
              <Typography variant="body2" color="text.secondary">
                All chain executions are running smoothly.
              </Typography>
            </Box>
          ) : (
            <Grid container spacing={2}>
              {interventions.map((execution) => (
                <Grid size={{ xs: 12, sm: 6 }} key={execution.id}>
                  <ExecutionCard
                    execution={execution}
                    onClick={() => {/* TODO: Navigate to detail */}}
                  />
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      </TabPanel>

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => {/* TODO: Navigate to detail */}}>
          <ListItemIcon>
            <ViewIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>View Details</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => {/* TODO: Edit */}}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit</ListItemText>
        </MenuItem>
        <MenuItem onClick={handlePublishToggle}>
          <ListItemIcon>
            {menuChain?.isPublished ? (
              <UnpublishIcon fontSize="small" />
            ) : (
              <PublishIcon fontSize="small" />
            )}
          </ListItemIcon>
          <ListItemText>
            {menuChain?.isPublished ? 'Unpublish' : 'Publish'}
          </ListItemText>
        </MenuItem>
        {menuChain?.isPublished && (
          <MenuItem onClick={() => {/* TODO: Start execution */}}>
            <ListItemIcon>
              <StartIcon fontSize="small" color="success" />
            </ListItemIcon>
            <ListItemText>Start Execution</ListItemText>
          </MenuItem>
        )}
        <MenuItem onClick={handleDeleteClick} sx={{ color: 'error.main' }}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText>Delete</ListItemText>
        </MenuItem>
      </Menu>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={handleDeleteCancel}>
        <DialogTitle>Delete Skill Chain</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the skill chain "{chainToDelete?.name}"?
            This action cannot be undone. All link configurations will be lost.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDeleteCancel}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
