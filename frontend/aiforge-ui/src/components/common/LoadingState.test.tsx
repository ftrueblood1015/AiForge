import { describe, it, expect } from 'vitest';
import { render, screen } from '../../test/test-utils';
import {
  LoadingSpinner,
  CardSkeleton,
  ListSkeleton,
  DetailSkeleton,
  TableSkeleton,
} from './LoadingState';

describe('LoadingSpinner', () => {
  it('renders a spinner', () => {
    render(<LoadingSpinner />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('renders with a message when provided', () => {
    render(<LoadingSpinner message="Loading data..." />);
    expect(screen.getByText('Loading data...')).toBeInTheDocument();
  });
});

describe('CardSkeleton', () => {
  it('renders default number of skeletons', () => {
    const { container } = render(<CardSkeleton />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons.length).toBe(3);
  });

  it('renders custom number of skeletons', () => {
    const { container } = render(<CardSkeleton count={5} />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons.length).toBe(5);
  });
});

describe('ListSkeleton', () => {
  it('renders default number of skeletons', () => {
    const { container } = render(<ListSkeleton />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons.length).toBe(5);
  });

  it('renders custom number of skeletons', () => {
    const { container } = render(<ListSkeleton count={3} />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons.length).toBe(3);
  });
});

describe('DetailSkeleton', () => {
  it('renders header and content skeletons', () => {
    const { container } = render(<DetailSkeleton />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    // Header has 3 skeletons (avatar + 2 text), plus sections have 2 each
    expect(skeletons.length).toBeGreaterThan(5);
  });
});

describe('TableSkeleton', () => {
  it('renders header and rows', () => {
    const { container } = render(<TableSkeleton rows={3} columns={4} />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    // 4 header columns + (3 rows * 4 columns) = 16
    expect(skeletons.length).toBe(16);
  });
});
