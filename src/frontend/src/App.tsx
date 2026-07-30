import React, { useMemo, useState } from 'react';
import { convert, getAudit, type ConversionResponse } from './api';

const CURRENCIES = [
  'USD',
  'EUR',
  'GBP',
  'JPY',
  'CHF',
  'CAD',
  'AUD',
  'CNY'
];

export default function App() {
  const [fromCurrency, setFromCurrency] = useState('USD');
  const [toCurrency, setToCurrency] = useState('EUR');
  const [amount, setAmount] = useState('100.00');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [conversion, setConversion] = useState<ConversionResponse | null>(null);

  const [auditId, setAuditId] = useState('');
  const [auditResult, setAuditResult] = useState<ConversionResponse | null>(null);
  const [auditBusy, setAuditBusy] = useState(false);
  const [auditError, setAuditError] = useState<string | null>(null);

  const formattedAmount = useMemo(() => {
    const n = Number(amount);
    if (!Number.isFinite(n)) return null;
    return n;
  }, [amount]);

  async function onConvert() {
    setBusy(true);
    setError(null);
    setConversion(null);
    try {
      if (formattedAmount === null) {
        setError('Enter a valid amount.');
        return;
      }

      const res = await convert({
        fromCurrency,
        toCurrency,
        amount: formattedAmount
      });
      setConversion(res);
      setAuditId(res.auditId);
      setAuditResult(null);
    } catch (e: any) {
      const details = e?.details;
      setError(details?.detail ?? e?.message ?? 'Conversion failed');
    } finally {
      setBusy(false);
    }
  }

  async function onLookup() {
    setAuditBusy(true);
    setAuditError(null);
    setAuditResult(null);
    try {
      if (!auditId.trim()) {
        setAuditError('Enter an auditId.');
        return;
      }

      const res = await getAudit(auditId.trim());
      setAuditResult(res);
    } catch (e: any) {
      const details = e?.details;
      setAuditError(details?.detail ?? e?.message ?? 'Audit lookup failed');
    } finally {
      setAuditBusy(false);
    }
  }

  const show = conversion ?? undefined;

  return (
    <div className="page">
      <div className="card">
        <h1 style={{ margin: 0, fontSize: 18 }}>Real-Time Currency Conversion & Audit Trail</h1>
        <p className="muted" style={{ marginTop: 8 }}>
          Convert instantly and get a backend-generated audit record you can retrieve later.
        </p>
      </div>

      <div style={{ height: 16 }} />

      <div className="grid">
        <div className="card">
          <div className="row">
            <div>
              <label>From currency</label>
              <select value={fromCurrency} onChange={(e) => setFromCurrency(e.target.value)}>
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label>To currency</label>
              <select value={toCurrency} onChange={(e) => setToCurrency(e.target.value)}>
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div style={{ height: 12 }} />

          <div>
            <label>Amount</label>
            <input
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              inputMode="decimal"
              aria-label="Amount"
            />
          </div>

          <div style={{ height: 12 }} />
          <button onClick={onConvert} disabled={busy}>
            {busy ? 'Converting…' : 'Convert'}
          </button>

          {error ? <div style={{ marginTop: 12 }} className="error">{error}</div> : null}

          {show ? (
            <div className="result">
              <div className="mono" style={{ fontSize: 13, opacity: 0.9 }}>
                Audit ID: {show.auditId}
              </div>
              <div style={{ marginTop: 10, fontSize: 16, fontWeight: 700 }}>
                {show.amount} {show.fromCurrency} = {show.convertedAmount} {show.toCurrency}
              </div>
              <div className="muted" style={{ marginTop: 8 }}>
                Exchange rate: {show.exchangeRate}
              </div>
              <div className="muted" style={{ marginTop: 4 }}>
                Provider date marker: {show.providerDateMarker}
              </div>
              <div className="muted" style={{ marginTop: 4 }}>
                Backend execution timestamp (UTC): {show.executionTimestampUtc}
              </div>
            </div>
          ) : null}
        </div>

        <div className="card">
          <div>
            <label>Audit lookup</label>
            <input
              value={auditId}
              onChange={(e) => setAuditId(e.target.value)}
              placeholder="Paste an auditId"
              className="mono"
            />
          </div>

          <div style={{ height: 12 }} />
          <button onClick={onLookup} disabled={auditBusy}>
            {auditBusy ? 'Looking up…' : 'Fetch audit record'}
          </button>

          {auditError ? <div style={{ marginTop: 12 }} className="error">{auditError}</div> : null}

          {auditResult ? (
            <div className="result">
              <div style={{ fontWeight: 700 }}>Retrieved record</div>
              <div className="muted" style={{ marginTop: 8 }}>
                {auditResult.amount} {auditResult.fromCurrency} → {auditResult.convertedAmount} {auditResult.toCurrency}
              </div>
              <div className="muted" style={{ marginTop: 4 }}>
                Rate: {auditResult.exchangeRate}
              </div>
              <div className="muted" style={{ marginTop: 4 }}>
                Provider date marker: {auditResult.providerDateMarker}
              </div>
              <div className="muted" style={{ marginTop: 4 }}>
                Backend execution timestamp (UTC): {auditResult.executionTimestampUtc}
              </div>
            </div>
          ) : null}

          {!auditResult && !auditError ? (
            <div className="muted" style={{ marginTop: 12, fontSize: 13 }}>
              After a conversion, the auditId is filled automatically so auditors can request it.
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}
