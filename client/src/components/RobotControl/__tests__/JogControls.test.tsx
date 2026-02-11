import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { JogControls } from '../JogControls';
import * as robotControlHook from '../../../hooks/useRobotControl';

// Mock the useRobotControl hook
vi.mock('../../../hooks/useRobotControl');

describe('JogControls', () => {
  const mockJog = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(robotControlHook.useRobotControl).mockReturnValue({
      jog: mockJog,
      isExecuting: false,
      moveToPosition: vi.fn(),
      emergencyStop: vi.fn(),
      home: vi.fn(),
      resetError: vi.fn(),
      lastError: null,
    });
  });

  it('renders all step size buttons', () => {
    render(<JogControls />);

    expect(screen.getByText('1 mm')).toBeInTheDocument();
    expect(screen.getByText('10 mm')).toBeInTheDocument();
    expect(screen.getByText('100 mm')).toBeInTheDocument();
  });

  it('selects 10mm step size by default', () => {
    render(<JogControls />);

    const button10mm = screen.getByText('10 mm');
    expect(button10mm).toHaveClass('active');
  });

  it('changes step size when button is clicked', () => {
    render(<JogControls />);

    const button100mm = screen.getByText('100 mm');
    fireEvent.click(button100mm);

    expect(button100mm).toHaveClass('active');
  });

  it('renders all jog control buttons', () => {
    render(<JogControls />);

    expect(screen.getByText('+X')).toBeInTheDocument();
    expect(screen.getByText('-X')).toBeInTheDocument();
    expect(screen.getByText('+Y')).toBeInTheDocument();
    expect(screen.getByText('-Y')).toBeInTheDocument();
    expect(screen.getByText('+Z ↑')).toBeInTheDocument();
    expect(screen.getByText('-Z ↓')).toBeInTheDocument();
  });

  it('calls jog with correct delta for +X button', async () => {
    render(<JogControls />);

    const plusXButton = screen.getByText('+X');
    fireEvent.click(plusXButton);

    await waitFor(() => {
      expect(mockJog).toHaveBeenCalledWith({
        deltaX: 10,
        deltaY: 0,
        deltaZ: 0,
      });
    });
  });

  it('calls jog with correct delta for -Y button', async () => {
    render(<JogControls />);

    const minusYButton = screen.getByText('-Y');
    fireEvent.click(minusYButton);

    await waitFor(() => {
      expect(mockJog).toHaveBeenCalledWith({
        deltaX: 0,
        deltaY: -10,
        deltaZ: 0,
      });
    });
  });

  it('uses selected step size for jog commands', async () => {
    render(<JogControls />);

    // Change to 100mm
    const button100mm = screen.getByText('100 mm');
    fireEvent.click(button100mm);

    // Click +Z
    const plusZButton = screen.getByText('+Z ↑');
    fireEvent.click(plusZButton);

    await waitFor(() => {
      expect(mockJog).toHaveBeenCalledWith({
        deltaX: 0,
        deltaY: 0,
        deltaZ: 100,
      });
    });
  });

  it('disables buttons when isExecuting is true', () => {
    vi.mocked(robotControlHook.useRobotControl).mockReturnValue({
      jog: mockJog,
      isExecuting: true,
      moveToPosition: vi.fn(),
      emergencyStop: vi.fn(),
      home: vi.fn(),
      resetError: vi.fn(),
      lastError: null,
    });

    render(<JogControls />);

    const plusXButton = screen.getByText('+X');
    expect(plusXButton).toBeDisabled();
  });
});
