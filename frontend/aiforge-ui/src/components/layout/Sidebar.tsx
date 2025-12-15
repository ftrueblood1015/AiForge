import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Divider,
  Box,
  Typography,
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  Folder as FolderIcon,
  Assignment as AssignmentIcon,
  Description as DescriptionIcon,
  Settings as SettingsIcon,
  Warning as DebtIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';

const DRAWER_WIDTH = 240;

interface SidebarProps {
  open: boolean;
}

const menuItems = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
  { text: 'Projects', icon: <FolderIcon />, path: '/projects' },
  { text: 'Tickets', icon: <AssignmentIcon />, path: '/tickets' },
  { text: 'Handoffs', icon: <DescriptionIcon />, path: '/handoffs' },
  { text: 'Tech Debt', icon: <DebtIcon />, path: '/debt' },
];

const bottomMenuItems = [
  { text: 'Settings', icon: <SettingsIcon />, path: '/settings' },
];

export default function Sidebar({ open }: SidebarProps) {
  const navigate = useNavigate();
  const location = useLocation();

  const isSelected = (path: string) => {
    if (path === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(path);
  };

  return (
    <Drawer
      variant="persistent"
      anchor="left"
      open={open}
      sx={{
        width: open ? DRAWER_WIDTH : 0,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
          borderRight: '1px solid',
          borderColor: 'divider',
        },
      }}
    >
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AssignmentIcon color="primary" />
          <Typography variant="h6" fontWeight={600}>
            AiForge
          </Typography>
        </Box>
      </Toolbar>
      <Divider />
      <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <List sx={{ flex: 1 }}>
          {menuItems.map((item) => (
            <ListItem key={item.text} disablePadding>
              <ListItemButton
                selected={isSelected(item.path)}
                onClick={() => navigate(item.path)}
                sx={{
                  mx: 1,
                  borderRadius: 1,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    color: 'primary.contrastText',
                    '&:hover': {
                      backgroundColor: 'primary.main',
                    },
                    '& .MuiListItemIcon-root': {
                      color: 'primary.contrastText',
                    },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
                <ListItemText primary={item.text} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
        <Divider />
        <List>
          {bottomMenuItems.map((item) => (
            <ListItem key={item.text} disablePadding>
              <ListItemButton
                selected={isSelected(item.path)}
                onClick={() => navigate(item.path)}
                sx={{ mx: 1, borderRadius: 1 }}
              >
                <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
                <ListItemText primary={item.text} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Box>
    </Drawer>
  );
}
