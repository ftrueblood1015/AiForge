import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { AppLayout } from './components/layout';
import { PageErrorBoundary } from './components/common';

// Lazy load pages for code splitting
const Dashboard = lazy(() => import('./pages/Dashboard'));
const ProjectList = lazy(() => import('./pages/ProjectList'));
const ProjectDetail = lazy(() => import('./pages/ProjectDetail'));
const TicketList = lazy(() => import('./pages/TicketList'));
const TicketDetail = lazy(() => import('./pages/TicketDetail'));
const HandoffList = lazy(() => import('./pages/HandoffList'));
const HandoffDetail = lazy(() => import('./pages/HandoffDetail'));
const Settings = lazy(() => import('./pages/Settings'));
const TechnicalDebtDashboard = lazy(() => import('./pages/TechnicalDebtDashboard'));
const AnalyticsDashboard = lazy(() => import('./pages/AnalyticsDashboard'));
const AgentList = lazy(() => import('./pages/AgentList'));
const SkillList = lazy(() => import('./pages/SkillList'));
const QueueList = lazy(() => import('./pages/QueueList'));
const QueueDetail = lazy(() => import('./pages/QueueDetail'));
const SkillChainList = lazy(() => import('./pages/SkillChainList'));

// Loading fallback component
function PageLoader() {
  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: 400,
      }}
    >
      <CircularProgress />
    </Box>
  );
}

function App() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />} errorElement={<PageErrorBoundary />}>
        <Route
          index
          element={
            <Suspense fallback={<PageLoader />}>
              <Dashboard />
            </Suspense>
          }
        />
        <Route
          path="projects"
          element={
            <Suspense fallback={<PageLoader />}>
              <ProjectList />
            </Suspense>
          }
        />
        <Route
          path="projects/:key"
          element={
            <Suspense fallback={<PageLoader />}>
              <ProjectDetail />
            </Suspense>
          }
        />
        <Route
          path="tickets"
          element={
            <Suspense fallback={<PageLoader />}>
              <TicketList />
            </Suspense>
          }
        />
        <Route
          path="tickets/:key"
          element={
            <Suspense fallback={<PageLoader />}>
              <TicketDetail />
            </Suspense>
          }
        />
        <Route
          path="handoffs"
          element={
            <Suspense fallback={<PageLoader />}>
              <HandoffList />
            </Suspense>
          }
        />
        <Route
          path="handoffs/:id"
          element={
            <Suspense fallback={<PageLoader />}>
              <HandoffDetail />
            </Suspense>
          }
        />
        <Route
          path="settings"
          element={
            <Suspense fallback={<PageLoader />}>
              <Settings />
            </Suspense>
          }
        />
        <Route
          path="debt"
          element={
            <Suspense fallback={<PageLoader />}>
              <TechnicalDebtDashboard />
            </Suspense>
          }
        />
        <Route
          path="analytics"
          element={
            <Suspense fallback={<PageLoader />}>
              <AnalyticsDashboard />
            </Suspense>
          }
        />
        <Route
          path="agents"
          element={
            <Suspense fallback={<PageLoader />}>
              <AgentList />
            </Suspense>
          }
        />
        <Route
          path="skills"
          element={
            <Suspense fallback={<PageLoader />}>
              <SkillList />
            </Suspense>
          }
        />
        <Route
          path="projects/:projectId/queues"
          element={
            <Suspense fallback={<PageLoader />}>
              <QueueList />
            </Suspense>
          }
        />
        <Route
          path="projects/:projectId/queues/:queueId"
          element={
            <Suspense fallback={<PageLoader />}>
              <QueueDetail />
            </Suspense>
          }
        />
        <Route
          path="skill-chains"
          element={
            <Suspense fallback={<PageLoader />}>
              <SkillChainList />
            </Suspense>
          }
        />
        <Route path="*" element={<PageErrorBoundary />} />
      </Route>
    </Routes>
  );
}

export default App;
