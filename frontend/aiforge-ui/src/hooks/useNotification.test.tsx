import { describe, it, expect, vi } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { SnackbarProvider } from 'notistack';
import { useNotification } from './useNotification';

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <SnackbarProvider maxSnack={3}>{children}</SnackbarProvider>
);

describe('useNotification', () => {
  it('provides notify function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.notify).toBeDefined();
    expect(typeof result.current.notify).toBe('function');
  });

  it('provides success function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.success).toBeDefined();
    expect(typeof result.current.success).toBe('function');
  });

  it('provides error function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.error).toBeDefined();
    expect(typeof result.current.error).toBe('function');
  });

  it('provides warning function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.warning).toBeDefined();
    expect(typeof result.current.warning).toBe('function');
  });

  it('provides info function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.info).toBeDefined();
    expect(typeof result.current.info).toBe('function');
  });

  it('provides close function', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    expect(result.current.close).toBeDefined();
    expect(typeof result.current.close).toBe('function');
  });

  it('can call notify without error', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    act(() => {
      result.current.notify('Test message');
    });
    // If we get here without an error, the test passes
  });

  it('can call success without error', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    act(() => {
      result.current.success('Success message');
    });
  });

  it('can call error without error', () => {
    const { result } = renderHook(() => useNotification(), { wrapper });
    act(() => {
      result.current.error('Error message');
    });
  });
});
