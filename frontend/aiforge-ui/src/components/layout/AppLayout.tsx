import { useState } from 'react';
import {
  Box,
  Toolbar,
  Dialog,
  DialogTitle,
  DialogContent,
  List,
  ListItem,
  ListItemText,
  Typography,
  Chip,
  IconButton,
} from '@mui/material';
import { Close as CloseIcon, Keyboard as KeyboardIcon } from '@mui/icons-material';
import { Outlet } from 'react-router-dom';
import { useUiStore } from '../../stores/uiStore';
import { useKeyboardShortcuts } from '../../hooks';
import Header from './Header';
import Sidebar from './Sidebar';

const DRAWER_WIDTH = 240;

export default function AppLayout() {
  const { sidebarOpen, toggleSidebar } = useUiStore();
  const shortcuts = useKeyboardShortcuts();
  const [shortcutsDialogOpen, setShortcutsDialogOpen] = useState(false);

  // Add keyboard shortcut to open the shortcuts dialog
  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === '?' && event.shiftKey) {
      setShortcutsDialogOpen(true);
    }
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }} onKeyDown={handleKeyDown} tabIndex={-1}>
      <Header
        sidebarOpen={sidebarOpen}
        onToggleSidebar={toggleSidebar}
        onShowShortcuts={() => setShortcutsDialogOpen(true)}
      />
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

      {/* Keyboard Shortcuts Dialog */}
      <Dialog
        open={shortcutsDialogOpen}
        onClose={() => setShortcutsDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <KeyboardIcon />
          Keyboard Shortcuts
          <IconButton
            sx={{ ml: 'auto' }}
            onClick={() => setShortcutsDialogOpen(false)}
            size="small"
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <List dense>
            {shortcuts.map((shortcut, index) => (
              <ListItem key={index} sx={{ py: 1 }}>
                <ListItemText primary={shortcut.description} />
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                  {shortcut.ctrlKey && <Chip label="Ctrl" size="small" variant="outlined" />}
                  {shortcut.altKey && <Chip label="Alt" size="small" variant="outlined" />}
                  {shortcut.shiftKey && <Chip label="Shift" size="small" variant="outlined" />}
                  <Chip
                    label={shortcut.key.toUpperCase()}
                    size="small"
                    color="primary"
                    variant="outlined"
                  />
                </Box>
              </ListItem>
            ))}
            <ListItem sx={{ py: 1 }}>
              <ListItemText primary="Show this dialog" />
              <Box sx={{ display: 'flex', gap: 0.5 }}>
                <Chip label="Shift" size="small" variant="outlined" />
                <Chip label="?" size="small" color="primary" variant="outlined" />
              </Box>
            </ListItem>
          </List>
          <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
            Note: Shortcuts are disabled when typing in input fields.
          </Typography>
        </DialogContent>
      </Dialog>
    </Box>
  );
}
