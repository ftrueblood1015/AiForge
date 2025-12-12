import { Box, Toolbar } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { useUiStore } from '../../stores/uiStore';
import Header from './Header';
import Sidebar from './Sidebar';

const DRAWER_WIDTH = 240;

export default function AppLayout() {
  const { sidebarOpen, toggleSidebar } = useUiStore();

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Header sidebarOpen={sidebarOpen} onToggleSidebar={toggleSidebar} />
      <Sidebar open={sidebarOpen} />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: sidebarOpen ? `calc(100% - ${DRAWER_WIDTH}px)` : '100%',
          ml: sidebarOpen ? 0 : `-${DRAWER_WIDTH}px`,
          backgroundColor: 'background.default',
          transition: (theme) =>
            theme.transitions.create(['width', 'margin'], {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
        }}
      >
        <Toolbar /> {/* Spacer for fixed AppBar */}
        <Outlet />
      </Box>
    </Box>
  );
}
