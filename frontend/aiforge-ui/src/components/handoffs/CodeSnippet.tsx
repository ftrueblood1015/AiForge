import { useState } from 'react';
import { Box, IconButton, Tooltip, Typography } from '@mui/material';
import { ContentCopy as CopyIcon, Check as CheckIcon } from '@mui/icons-material';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';

interface CodeSnippetProps {
  code: string;
  language: string;
  showLineNumbers?: boolean;
  maxHeight?: number | string;
  title?: string;
}

// Map common language aliases to Prism-supported names
const languageMap: Record<string, string> = {
  cs: 'csharp',
  'c#': 'csharp',
  ts: 'typescript',
  tsx: 'tsx',
  js: 'javascript',
  jsx: 'jsx',
  py: 'python',
  rb: 'ruby',
  yml: 'yaml',
  md: 'markdown',
  sh: 'bash',
  shell: 'bash',
  text: 'plaintext',
};

export default function CodeSnippet({
  code,
  language,
  showLineNumbers = true,
  maxHeight = 400,
  title,
}: CodeSnippetProps) {
  const [copied, setCopied] = useState(false);

  const normalizedLanguage = languageMap[language.toLowerCase()] || language.toLowerCase();

  const handleCopy = async () => {
    await navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <Box sx={{ position: 'relative' }}>
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          bgcolor: 'grey.800',
          px: 2,
          py: 0.5,
          borderTopLeftRadius: 4,
          borderTopRightRadius: 4,
        }}
      >
        <Typography variant="caption" color="grey.400">
          {title || normalizedLanguage}
        </Typography>
        <Tooltip title={copied ? 'Copied!' : 'Copy code'}>
          <IconButton size="small" onClick={handleCopy} sx={{ color: 'grey.400' }}>
            {copied ? <CheckIcon fontSize="small" /> : <CopyIcon fontSize="small" />}
          </IconButton>
        </Tooltip>
      </Box>

      {/* Code */}
      <Box sx={{ maxHeight, overflow: 'auto' }}>
        <SyntaxHighlighter
          language={normalizedLanguage}
          style={vscDarkPlus}
          showLineNumbers={showLineNumbers}
          customStyle={{
            margin: 0,
            borderTopLeftRadius: 0,
            borderTopRightRadius: 0,
            borderBottomLeftRadius: 4,
            borderBottomRightRadius: 4,
          }}
          lineNumberStyle={{
            minWidth: '3em',
            paddingRight: '1em',
            color: '#858585',
            textAlign: 'right',
          }}
        >
          {code}
        </SyntaxHighlighter>
      </Box>
    </Box>
  );
}
