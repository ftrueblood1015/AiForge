import { describe, it, expect } from 'vitest';
import { render, screen } from '../../test/test-utils';
import StatusChip from './StatusChip';

describe('StatusChip', () => {
  it('renders ToDo status correctly', () => {
    render(<StatusChip status="ToDo" />);
    expect(screen.getByText('To Do')).toBeInTheDocument();
  });

  it('renders InProgress status correctly', () => {
    render(<StatusChip status="InProgress" />);
    expect(screen.getByText('In Progress')).toBeInTheDocument();
  });

  it('renders InReview status correctly', () => {
    render(<StatusChip status="InReview" />);
    expect(screen.getByText('In Review')).toBeInTheDocument();
  });

  it('renders Done status correctly', () => {
    render(<StatusChip status="Done" />);
    expect(screen.getByText('Done')).toBeInTheDocument();
  });
});
