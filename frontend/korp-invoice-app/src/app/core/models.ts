export interface Product {
  id: string;
  code: string;
  description: string;
  balance: number;
}

export interface InvoiceItem {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface Invoice {
  id: string;
  number: number;
  status: 'Aberta' | 'Fechada';
  createdAtUtc: string;
  closedAtUtc: string | null;
  lastProcessingError: string | null;
  items: InvoiceItem[];
}

export interface CloseInvoiceResponse {
  invoice: Invoice;
  alreadyClosed: boolean;
  stockOperationAlreadyProcessed: boolean;
}

export interface FailureSimulation {
  enabled: boolean;
}
