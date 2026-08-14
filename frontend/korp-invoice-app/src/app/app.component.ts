import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormArray,
  UntypedFormBuilder,
  Validators
} from '@angular/forms';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { ApiService } from './core/api.service';
import { Invoice, Product } from './core/models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly formBuilder = inject(UntypedFormBuilder);
  private readonly changeDetector = inject(ChangeDetectorRef);

  readonly productForm = this.formBuilder.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    balance: [0, [Validators.required, Validators.min(0)]]
  });

  readonly invoiceForm = this.formBuilder.group({
    items: this.formBuilder.array([this.createInvoiceItemGroup()])
  });

  products: Product[] = [];
  invoices: Invoice[] = [];
  activeSection: 'products' | 'invoices' = 'products';
  loading = true;
  savingProduct = false;
  savingInvoice = false;
  processingInvoiceId: string | null = null;
  failureSimulationEnabled = false;
  togglingFailure = false;
  message = '';
  errorMessage = '';
  invoiceForPrint: Invoice | null = null;
  expandedInvoiceId: string | null = null;

  get invoiceItems(): UntypedFormArray {
    return this.invoiceForm.get('items') as UntypedFormArray;
  }

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.clearFeedback();

    forkJoin({
      products: this.api.listProducts(),
      invoices: this.api.listInvoices(),
      failure: this.api.getFailureSimulation()
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.refreshView();
      }))
      .subscribe({
        next: ({ products, invoices, failure }) => {
          this.products = products;
          this.invoices = invoices;
          this.failureSimulationEnabled = failure.enabled;
        },
        error: (error: HttpErrorResponse) => this.showError(error)
      });
  }

  saveProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.savingProduct = true;
    this.clearFeedback();
    const value = this.productForm.getRawValue();

    this.api
      .createProduct({
        code: String(value.code),
        description: String(value.description),
        balance: Number(value.balance)
      })
      .pipe(
        switchMap(() => this.api.listProducts()),
        finalize(() => {
          this.savingProduct = false;
          this.refreshView();
        })
      )
      .subscribe({
        next: (products) => {
          this.products = products;
          this.productForm.reset({ code: '', description: '', balance: 0 });
          this.message = 'Produto cadastrado com sucesso.';
        },
        error: (error: HttpErrorResponse) => this.showError(error)
      });
  }

  addInvoiceItem(): void {
    this.invoiceItems.push(this.createInvoiceItemGroup());
  }

  removeInvoiceItem(index: number): void {
    if (this.invoiceItems.length > 1) {
      this.invoiceItems.removeAt(index);
    }
  }

  saveInvoice(): void {
    if (this.invoiceForm.invalid) {
      this.invoiceForm.markAllAsTouched();
      return;
    }

    this.savingInvoice = true;
    this.clearFeedback();
    const items = this.invoiceItems.getRawValue().map((item: Record<string, unknown>) => ({
      productId: String(item['productId']),
      quantity: Number(item['quantity'])
    }));

    this.api
      .createInvoice(items)
      .pipe(
        switchMap(() => this.api.listInvoices()),
        finalize(() => {
          this.savingInvoice = false;
          this.refreshView();
        })
      )
      .subscribe({
        next: (invoices) => {
          this.invoices = invoices;
          this.invoiceItems.clear();
          this.invoiceItems.push(this.createInvoiceItemGroup());
          this.message = 'Nota fiscal criada com status Aberta.';
        },
        error: (error: HttpErrorResponse) => this.showError(error)
      });
  }

  printInvoice(invoice: Invoice): void {
    if (invoice.status !== 'Aberta' || this.processingInvoiceId) {
      return;
    }

    this.processingInvoiceId = invoice.id;
    this.clearFeedback();

    this.api
      .closeInvoice(invoice.id)
      .pipe(
        switchMap((result) =>
          forkJoin({
            result: of(result),
            invoices: this.api.listInvoices(),
            products: this.api.listProducts()
          })
        ),
        finalize(() => {
          this.processingInvoiceId = null;
          this.refreshView();
        })
      )
      .subscribe({
        next: ({ result, invoices, products }) => {
          this.invoices = invoices;
          this.products = products;
          this.invoiceForPrint = result.invoice;
          this.message = 'Nota fechada e estoque atualizado. Abrindo impressão.';
          this.openPrintDialog();
        },
        error: (error: HttpErrorResponse) => this.showError(error)
      });
  }

  toggleFailureSimulation(): void {
    this.togglingFailure = true;
    this.clearFeedback();
    const nextValue = !this.failureSimulationEnabled;

    this.api
      .setFailureSimulation(nextValue)
      .pipe(finalize(() => {
        this.togglingFailure = false;
        this.refreshView();
      }))
      .subscribe({
        next: ({ enabled }) => {
          this.failureSimulationEnabled = enabled;
          this.message = enabled
            ? 'Falha do Estoque ativada. Tente imprimir uma nota aberta.'
            : 'Estoque recuperado. A nota aberta já pode ser processada novamente.';
        },
        error: (error: HttpErrorResponse) => this.showError(error)
      });
  }

  trackById(_: number, item: Product | Invoice): string {
    return item.id;
  }

  toggleInvoiceDetails(invoiceId: string): void {
    this.expandedInvoiceId = this.expandedInvoiceId === invoiceId ? null : invoiceId;
  }

  getInvoiceTotalQuantity(invoice: Invoice): number {
    return invoice.items.reduce((total, item) => total + item.quantity, 0);
  }

  private createInvoiceItemGroup() {
    return this.formBuilder.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  private clearFeedback(): void {
    this.message = '';
    this.errorMessage = '';
  }

  private showError(error: HttpErrorResponse): void {
    this.errorMessage =
      error.error?.detail ??
      error.error?.title ??
      'Não foi possível concluir a operação. Verifique se os serviços estão em execução.';
    this.refreshView();
  }

  private refreshView(): void {
    this.changeDetector.markForCheck();
  }

  private openPrintDialog(): void {
    // Garante que o template oculto seja inserido no DOM antes de ativar a mídia de impressão.
    this.changeDetector.detectChanges();

    requestAnimationFrame(() => {
      requestAnimationFrame(() => window.print());
    });
  }
}
