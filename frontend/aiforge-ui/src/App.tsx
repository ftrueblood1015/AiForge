import { Routes, Route } from 'react-router-dom';
import { AppLayout } from './components/layout';
import {
  Dashboard,
  ProjectList,
  ProjectDetail,
  TicketList,
  TicketDetail,
  HandoffList,
  HandoffDetail,
  Settings,
} from './pages';

function App() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />}>
        <Route index element={<Dashboard />} />
        <Route path="projects" element={<ProjectList />} />
        <Route path="projects/:key" element={<ProjectDetail />} />
        <Route path="tickets" element={<TicketList />} />
        <Route path="tickets/:key" element={<TicketDetail />} />
        <Route path="handoffs" element={<HandoffList />} />
        <Route path="handoffs/:id" element={<HandoffDetail />} />
        <Route path="settings" element={<Settings />} />
      </Route>
    </Routes>
  );
}

export default App;
