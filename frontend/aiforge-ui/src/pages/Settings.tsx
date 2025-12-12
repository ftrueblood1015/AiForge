import {
  Box,
  Typography,
  Card,
  CardContent,
  ToggleButtonGroup,
  ToggleButton,
  Divider,
} from '@mui/material';
import {
  LightMode as LightIcon,
  DarkMode as DarkIcon,
  SettingsBrightness as SystemIcon,
} from '@mui/icons-material';
import { useUiStore } from '../stores/uiStore';
import type { ThemeMode } from '../theme';

export default function Settings() {
  const { themeMode, setThemeMode } = useUiStore();

  const handleThemeChange = (_: React.MouseEvent<HTMLElement>, newMode: ThemeMode | null) => {
    if (newMode !== null) {
      setThemeMode(newMode);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Settings
      </Typography>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Appearance
          </Typography>
          <Divider sx={{ mb: 3 }} />

          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Theme
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Select your preferred color theme for the application.
            </Typography>

            <ToggleButtonGroup
              value={themeMode}
              exclusive
              onChange={handleThemeChange}
              aria-label="theme mode"
              sx={{
                display: 'flex',
                flexWrap: 'wrap',
                gap: 1,
                '& .MuiToggleButton-root': {
                  flex: '1 1 auto',
                  minWidth: 120,
                  py: 2,
                  px: 3,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 1,
                  border: 1,
                  borderColor: 'divider',
                  '&.Mui-selected': {
                    borderColor: 'primary.main',
                    bgcolor: 'primary.main',
                    color: 'primary.contrastText',
                    '&:hover': {
                      bgcolor: 'primary.dark',
                    },
                  },
                },
              }}
            >
              <ToggleButton value="light" aria-label="light theme">
                <LightIcon />
                <Typography variant="body2">Light</Typography>
              </ToggleButton>
              <ToggleButton value="dark" aria-label="dark theme">
                <DarkIcon />
                <Typography variant="body2">Dark</Typography>
              </ToggleButton>
              <ToggleButton value="system" aria-label="system theme">
                <SystemIcon />
                <Typography variant="body2">System</Typography>
              </ToggleButton>
            </ToggleButtonGroup>

            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
              {themeMode === 'system'
                ? 'Theme will automatically match your system preferences.'
                : themeMode === 'dark'
                  ? 'Dark theme is enabled.'
                  : 'Light theme is enabled.'}
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
