import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App';

describe('App', () => {
  it('renders conversion form', () => {
    render(<App />);
    expect(screen.getByText('Convert')).toBeTruthy();
  });

  it('calls conversion endpoint on convert', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        auditId: 'a1',
        fromCurrency: 'USD',
        toCurrency: 'EUR',
        amount: 100,
        exchangeRate: 0.9,
        convertedAmount: 90,
        providerDateMarker: '2026-01-04',
        executionTimestampUtc: new Date().toISOString()
      })
    }));

    render(<App />);
    const user = userEvent.setup();
    const buttons = screen.getAllByText('Convert');
    await user.click(buttons[0]);
    expect(globalThis.fetch).toHaveBeenCalled();
  });
});
