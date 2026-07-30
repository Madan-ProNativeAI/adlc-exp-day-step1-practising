export type ConversionRequest = {
  fromCurrency: string;
  toCurrency: string;
  amount: number;
};

export type ConversionResponse = {
  auditId: string;
  fromCurrency: string;
  toCurrency: string;
  amount: number;
  exchangeRate: number;
  convertedAmount: number;
  providerDateMarker: string;
  executionTimestampUtc: string;
};

export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
};

function getApiBase(): string {
  const v = window.__VITE_API_URL__ ?? '';
  return v;
}

export async function convert(req: ConversionRequest): Promise<ConversionResponse> {
  const base = getApiBase();
  const res = await fetch(`${base}/api/conversions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const details = (await res.json().catch(() => ({}))) as ProblemDetails;
    const e: any = new Error(details.title ?? 'Conversion failed');
    e.details = details;
    throw e;
  }

  return (await res.json()) as ConversionResponse;
}

export async function getAudit(auditId: string): Promise<ConversionResponse> {
  const base = getApiBase();
  const res = await fetch(`${base}/api/audits/${encodeURIComponent(auditId)}`);
  if (!res.ok) {
    const details = (await res.json().catch(() => ({}))) as ProblemDetails;
    const e: any = new Error(details.title ?? 'Audit lookup failed');
    e.details = details;
    throw e;
  }
  return (await res.json()) as ConversionResponse;
}
