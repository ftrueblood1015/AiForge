import { useTheme } from '@mui/material/styles';
import type { SxProps, Theme } from '@mui/material/styles';

/**
 * Hook that returns theme-aware styles for markdown content.
 * Ensures proper contrast in both light and dark modes.
 */
export function useMarkdownStyles(): SxProps<Theme> {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  // Background colors that adapt to theme
  const codeBg = isDark ? 'grey.800' : 'grey.100';
  const tableBg = isDark ? 'grey.800' : 'grey.100';

  return {
    '& h1': { fontSize: '1.5rem', fontWeight: 600, mt: 2, mb: 1 },
    '& h2': { fontSize: '1.25rem', fontWeight: 600, mt: 2, mb: 1 },
    '& h3': { fontSize: '1.1rem', fontWeight: 600, mt: 1.5, mb: 0.5 },
    '& p': { mb: 1.5 },
    '& ul, & ol': { pl: 3, mb: 1.5 },
    '& li': { mb: 0.5 },
    '& code': {
      bgcolor: codeBg,
      px: 0.5,
      py: 0.25,
      borderRadius: 0.5,
      fontFamily: 'monospace',
      fontSize: '0.875em',
    },
    '& pre': {
      bgcolor: 'grey.900',
      color: 'grey.100',
      p: 2,
      borderRadius: 1,
      overflow: 'auto',
      '& code': {
        bgcolor: 'transparent',
        p: 0,
      },
    },
    '& blockquote': {
      borderLeft: 4,
      borderColor: 'primary.main',
      pl: 2,
      ml: 0,
      color: 'text.secondary',
    },
    '& table': {
      width: '100%',
      borderCollapse: 'collapse',
      mb: 2,
    },
    '& th, & td': {
      border: 1,
      borderColor: 'divider',
      p: 1,
    },
    '& th': {
      bgcolor: tableBg,
      fontWeight: 600,
    },
    '& a': {
      color: 'primary.main',
      textDecoration: 'none',
      '&:hover': {
        textDecoration: 'underline',
      },
    },
  };
}

/**
 * Compact version of markdown styles for smaller contexts (comments, cards).
 */
export function useCompactMarkdownStyles(): SxProps<Theme> {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const codeBg = isDark ? 'grey.800' : 'grey.100';

  return {
    '& p': { mb: 1, fontSize: '0.875rem', '&:last-child': { mb: 0 } },
    '& h1, & h2, & h3': { fontSize: '1rem', fontWeight: 600, mt: 1, mb: 0.5 },
    '& ul, & ol': { pl: 2.5, mb: 1, fontSize: '0.875rem' },
    '& li': { mb: 0.25 },
    '& code': {
      bgcolor: codeBg,
      px: 0.5,
      py: 0.25,
      borderRadius: 0.5,
      fontFamily: 'monospace',
      fontSize: '0.8rem',
    },
    '& pre': {
      bgcolor: 'grey.900',
      color: 'grey.100',
      p: 1.5,
      borderRadius: 1,
      overflow: 'auto',
      fontSize: '0.8rem',
      '& code': { bgcolor: 'transparent', p: 0 },
    },
    '& blockquote': {
      borderLeft: 3,
      borderColor: 'primary.main',
      pl: 1.5,
      ml: 0,
      color: 'text.secondary',
    },
    '& a': {
      color: 'primary.main',
      textDecoration: 'none',
      '&:hover': { textDecoration: 'underline' },
    },
  };
}
