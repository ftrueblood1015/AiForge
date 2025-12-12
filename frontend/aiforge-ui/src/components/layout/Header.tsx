import {
  AppBar,
  Toolbar,
  IconButton,
  InputBase,
  Box,
  Avatar,
  Tooltip,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Search as SearchIcon,
  Notifications as NotificationsIcon,
  Keyboard as KeyboardIcon,
} from '@mui/icons-material';
import { alpha, styled } from '@mui/material/styles';

const DRAWER_WIDTH = 240;

const Search = styled('div')(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  backgroundColor: alpha(theme.palette.common.black, 0.05),
  '&:hover': {
    backgroundColor: alpha(theme.palette.common.black, 0.1),
  },
  marginRight: theme.spacing(2),
  marginLeft: 0,
  width: '100%',
  [theme.breakpoints.up('sm')]: {
    marginLeft: theme.spacing(3),
    width: 'auto',
  },
}));

const SearchIconWrapper = styled('div')(({ theme }) => ({
  padding: theme.spacing(0, 2),
  height: '100%',
  position: 'absolute',
  pointerEvents: 'none',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  color: theme.palette.text.secondary,
}));

const StyledInputBase = styled(InputBase)(({ theme }) => ({
  color: 'inherit',
  '& .MuiInputBase-input': {
    padding: theme.spacing(1, 1, 1, 0),
    paddingLeft: `calc(1em + ${theme.spacing(4)})`,
    transition: theme.transitions.create('width'),
    width: '100%',
    [theme.breakpoints.up('md')]: {
      width: '30ch',
      '&:focus': {
        width: '40ch',
      },
    },
  },
}));

interface HeaderProps {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
  onShowShortcuts?: () => void;
}

export default function Header({ sidebarOpen, onToggleSidebar, onShowShortcuts }: HeaderProps) {
  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        width: sidebarOpen ? `calc(100% - ${DRAWER_WIDTH}px)` : '100%',
        ml: sidebarOpen ? `${DRAWER_WIDTH}px` : 0,
        backgroundColor: 'background.paper',
        color: 'text.primary',
        borderBottom: '1px solid',
        borderColor: 'divider',
        transition: (theme) =>
          theme.transitions.create(['width', 'margin'], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
          }),
      }}
    >
      <Toolbar>
        <IconButton
          color="inherit"
          aria-label="toggle sidebar"
          onClick={onToggleSidebar}
          edge="start"
          sx={{ mr: 2 }}
        >
          <MenuIcon />
        </IconButton>

        <Search>
          <SearchIconWrapper>
            <SearchIcon />
          </SearchIconWrapper>
          <StyledInputBase
            placeholder="Search tickets, projects..."
            inputProps={{ 'aria-label': 'search' }}
          />
        </Search>

        <Box sx={{ flexGrow: 1 }} />

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Tooltip title="Keyboard shortcuts (Shift+?)">
            <IconButton color="inherit" onClick={onShowShortcuts} aria-label="keyboard shortcuts">
              <KeyboardIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Notifications">
            <IconButton color="inherit" aria-label="notifications">
              <NotificationsIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Account">
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
              U
            </Avatar>
          </Tooltip>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
