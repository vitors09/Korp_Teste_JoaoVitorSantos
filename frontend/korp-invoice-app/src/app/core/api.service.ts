import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CloseInvoiceResponse,
  FailureSimulation,
  Invoice,
  Product
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  listProducts(): Observable<Product[]> {
    return this.http.get<Product[]>('/inventory/api/products');
  }

  createProduct(payload: {
    code: string;
    description: string;
    balance: number;
  }): Observable<Product> {
    return this.http.post<Product>('/inventory/api/products', payload);
  }

  listInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>('/billing/api/invoices');
  }

  createInvoice(items: Array<{ productId: string; quantity: number }>): Observable<Invoice> {
    return this.http.post<Invoice>('/billing/api/invoices', { items });
  }

  closeInvoice(id: string): Observable<CloseInvoiceResponse> {
    return this.http.post<CloseInvoiceResponse>(`/billing/api/invoices/${id}/close`, {});
  }

  getFailureSimulation(): Observable<FailureSimulation> {
    return this.http.get<FailureSimulation>('/inventory/api/system/failure-simulation');
  }

  setFailureSimulation(enabled: boolean): Observable<FailureSimulation> {
    return this.http.put<FailureSimulation>('/inventory/api/system/failure-simulation', { enabled });
  }
}
