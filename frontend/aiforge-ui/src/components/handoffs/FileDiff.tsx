import { useState } from 'react';
import { Box, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import {
  Compare as DiffIcon,
  ViewColumn as SplitIcon,
  ViewStream as UnifiedIcon,
} from '@mui/icons-material';
import { diffLines, type Change } from 'diff';
import CodeSnippet from './CodeSnippet';

interface FileDiffProps {
  before: string;
  after: string;
  language: string;
}

type DiffViewMode = 'unified' | 'split' | 'before' | 'after';

export default function FileDiff({ before, after, language }: FileDiffProps) {
  const [viewMode, setViewMode] = useState<DiffViewMode>('unified');

  const changes = diffLines(before, after);

  const handleViewModeChange = (_: React.MouseEvent<HTMLElement>, newMode: DiffViewMode | null) => {
    if (newMode) {
      setViewMode(newMode);
    }
  };

  // Count additions and deletions
  const stats = changes.reduce(
    (acc, change) => {
      if (change.added) {
        acc.additions += change.count || 0;
      } else if (change.removed) {
        acc.deletions += change.count || 0;
      }
      return acc;
    },
    { additions: 0, deletions: 0 }
  );

  return (
    <Box>
      {/* Controls */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <DiffIcon color="action" />
          <Typography variant="body2">
            <Box component="span" sx={{ color: 'success.main', fontWeight: 600 }}>
              +{stats.additions}
            </Box>
            {' / '}
            <Box component="span" sx={{ color: 'error.main', fontWeight: 600 }}>
              -{stats.deletions}
            </Box>
            {' lines changed'}
          </Typography>
        </Box>
        <ToggleButtonGroup
          value={viewMode}
          exclusive
          onChange={handleViewModeChange}
          size="small"
        >
          <ToggleButton value="unified">
            <UnifiedIcon fontSize="small" sx={{ mr: 0.5 }} />
            Unified
          </ToggleButton>
          <ToggleButton value="split">
            <SplitIcon fontSize="small" sx={{ mr: 0.5 }} />
            Split
          </ToggleButton>
          <ToggleButton value="before">Before</ToggleButton>
          <ToggleButton value="after">After</ToggleButton>
        </ToggleButtonGroup>
      </Box>

      {/* Diff Views */}
      {viewMode === 'unified' && <UnifiedDiffView changes={changes} />}
      {viewMode === 'split' && <SplitDiffView changes={changes} />}
      {viewMode === 'before' && <CodeSnippet code={before} language={language} title="Before" />}
      {viewMode === 'after' && <CodeSnippet code={after} language={language} title="After" />}
    </Box>
  );
}

interface UnifiedDiffViewProps {
  changes: Change[];
}

function UnifiedDiffView({ changes }: UnifiedDiffViewProps) {
  let oldLineNum = 1;
  let newLineNum = 1;

  return (
    <Box
      sx={{
        fontFamily: 'monospace',
        fontSize: '0.8rem',
        bgcolor: 'grey.900',
        borderRadius: 1,
        overflow: 'auto',
        maxHeight: 500,
      }}
    >
      {changes.map((change, changeIndex) => {
        const lines = change.value.split('\n').filter((_, i, arr) =>
          // Keep all lines except trailing empty line from split
          i < arr.length - 1 || arr[i] !== ''
        );

        return lines.map((line, lineIndex) => {
          const key = `${changeIndex}-${lineIndex}`;
          let bgcolor = 'transparent';
          let prefix = ' ';
          let oldNum: number | string = '';
          let newNum: number | string = '';

          if (change.added) {
            bgcolor = 'rgba(46, 160, 67, 0.2)';
            prefix = '+';
            newNum = newLineNum++;
          } else if (change.removed) {
            bgcolor = 'rgba(248, 81, 73, 0.2)';
            prefix = '-';
            oldNum = oldLineNum++;
          } else {
            oldNum = oldLineNum++;
            newNum = newLineNum++;
          }

          return (
            <Box
              key={key}
              sx={{
                display: 'flex',
                bgcolor,
                '&:hover': { bgcolor: change.added || change.removed ? bgcolor : 'rgba(255,255,255,0.05)' },
              }}
            >
              {/* Line numbers */}
              <Box
                sx={{
                  width: 50,
                  minWidth: 50,
                  color: 'grey.600',
                  textAlign: 'right',
                  pr: 1,
                  userSelect: 'none',
                  borderRight: 1,
                  borderColor: 'grey.800',
                }}
              >
                {oldNum}
              </Box>
              <Box
                sx={{
                  width: 50,
                  minWidth: 50,
                  color: 'grey.600',
                  textAlign: 'right',
                  pr: 1,
                  userSelect: 'none',
                  borderRight: 1,
                  borderColor: 'grey.800',
                }}
              >
                {newNum}
              </Box>
              {/* Prefix */}
              <Box
                sx={{
                  width: 20,
                  minWidth: 20,
                  textAlign: 'center',
                  color: change.added ? 'success.main' : change.removed ? 'error.main' : 'grey.500',
                  fontWeight: 600,
                }}
              >
                {prefix}
              </Box>
              {/* Content */}
              <Box
                sx={{
                  flex: 1,
                  color: 'grey.100',
                  whiteSpace: 'pre',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  pl: 1,
                }}
              >
                {line || ' '}
              </Box>
            </Box>
          );
        });
      })}
    </Box>
  );
}

interface SplitDiffViewProps {
  changes: Change[];
}

function SplitDiffView({ changes }: SplitDiffViewProps) {
  // Build left (before) and right (after) panels
  const leftLines: { num: number; content: string; type: 'normal' | 'removed' | 'empty' }[] = [];
  const rightLines: { num: number; content: string; type: 'normal' | 'added' | 'empty' }[] = [];

  let oldLineNum = 1;
  let newLineNum = 1;

  changes.forEach((change) => {
    const lines = change.value.split('\n').filter((_, i, arr) =>
      i < arr.length - 1 || arr[i] !== ''
    );

    if (change.added) {
      lines.forEach((line) => {
        leftLines.push({ num: 0, content: '', type: 'empty' });
        rightLines.push({ num: newLineNum++, content: line, type: 'added' });
      });
    } else if (change.removed) {
      lines.forEach((line) => {
        leftLines.push({ num: oldLineNum++, content: line, type: 'removed' });
        rightLines.push({ num: 0, content: '', type: 'empty' });
      });
    } else {
      lines.forEach((line) => {
        leftLines.push({ num: oldLineNum++, content: line, type: 'normal' });
        rightLines.push({ num: newLineNum++, content: line, type: 'normal' });
      });
    }
  });

  const renderPanel = (
    lines: { num: number; content: string; type: string }[],
    side: 'left' | 'right'
  ) => (
    <Box sx={{ flex: 1, overflow: 'auto' }}>
      {lines.map((line, index) => {
        let bgcolor = 'transparent';
        if (line.type === 'added') bgcolor = 'rgba(46, 160, 67, 0.2)';
        if (line.type === 'removed') bgcolor = 'rgba(248, 81, 73, 0.2)';
        if (line.type === 'empty') bgcolor = 'rgba(128, 128, 128, 0.1)';

        return (
          <Box
            key={`${side}-${index}`}
            sx={{
              display: 'flex',
              bgcolor,
              minHeight: '1.4em',
            }}
          >
            <Box
              sx={{
                width: 40,
                minWidth: 40,
                color: 'grey.600',
                textAlign: 'right',
                pr: 1,
                userSelect: 'none',
                borderRight: 1,
                borderColor: 'grey.800',
              }}
            >
              {line.num || ''}
            </Box>
            <Box
              sx={{
                flex: 1,
                color: 'grey.100',
                whiteSpace: 'pre',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                pl: 1,
              }}
            >
              {line.content || ' '}
            </Box>
          </Box>
        );
      })}
    </Box>
  );

  return (
    <Box
      sx={{
        display: 'flex',
        fontFamily: 'monospace',
        fontSize: '0.8rem',
        bgcolor: 'grey.900',
        borderRadius: 1,
        overflow: 'auto',
        maxHeight: 500,
      }}
    >
      {renderPanel(leftLines, 'left')}
      <Box sx={{ width: 1, bgcolor: 'grey.700' }} />
      {renderPanel(rightLines, 'right')}
    </Box>
  );
}
