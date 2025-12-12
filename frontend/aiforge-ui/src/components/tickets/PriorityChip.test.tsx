import { describe, it, expect } from 'vitest';
import { render, screen } from '../../test/test-utils';
import PriorityChip from './PriorityChip';

describe('PriorityChip', () => {
  it('renders Low priority correctly', () => {
    render(<PriorityChip priority="Low" />);
    expect(screen.getByText('Low')).toBeInTheDocument();
  });

  it('renders Medium priority correctly', () => {
    render(<PriorityChip priority="Medium" />);
    expect(screen.getByText('Medium')).toBeInTheDocument();
  });

  it('renders High priority correctly', () => {
    render(<PriorityChip priority="High" />);
    expect(screen.getByText('High')).toBeInTheDocument();
  });

  it('renders Critical priority correctly', () => {
    render(<PriorityChip priority="Critical" />);
    expect(screen.getByText('Critical')).toBeInTheDocument();
  });
});
