import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { IncomeService, Income } from '../services/income.service';
import { DeleteConfirmDialogComponent } from '../../../shared/components/delete-confirm-dialog/delete-confirm-dialog.component';

@Component({
  selector: 'app-income-list',
  templateUrl: './income-list.component.html',
  styleUrls: ['./income-list.component.scss']
})
export class IncomeListComponent implements OnInit {
  allIncomes: Income[] = [];
  filteredIncomes: Income[] = [];
  isLoading = true;
  errorMessage: string | null = null;
  filterForm!: FormGroup;

  totalIncome = 0;
  thisMonthTotal = 0;

  constructor(
    private incomeService: IncomeService,
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.initFilterForm();
    this.loadIncomes();
  }

  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      sourceSearch: [''],
      startDate: [null],
      endDate: [null]
    });

    this.filterForm.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  loadIncomes(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.incomeService.getAllIncome().subscribe({
      next: (data) => {
        this.allIncomes = data;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load income records.';
      }
    });
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    const sourceQuery = filters.sourceSearch ? filters.sourceSearch.trim().toLowerCase() : '';
    const start = filters.startDate ? new Date(filters.startDate) : null;
    const end = filters.endDate ? new Date(filters.endDate) : null;

    if (start) start.setHours(0, 0, 0, 0);
    if (end) end.setHours(23, 59, 59, 999);

    this.filteredIncomes = this.allIncomes.filter(i => {
      let matches = true;

      // Source Filter
      if (sourceQuery) {
        matches = matches && i.source.toLowerCase().includes(sourceQuery);
      }

      // Date Range Filter
      const incDate = new Date(i.incomeDate);
      if (start) {
        matches = matches && incDate >= start;
      }
      if (end) {
        matches = matches && incDate <= end;
      }

      return matches;
    });

    this.calculateTotals();
  }

  private calculateTotals(): void {
    this.totalIncome = this.filteredIncomes.reduce((sum, i) => sum + i.amount, 0);

    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();

    this.thisMonthTotal = this.allIncomes
      .filter(i => {
        const d = new Date(i.incomeDate);
        return d.getMonth() === currentMonth && d.getFullYear() === currentYear;
      })
      .reduce((sum, i) => sum + i.amount, 0);
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      sourceSearch: '',
      startDate: null,
      endDate: null
    });
  }

  onDelete(id: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Income Record',
        message: 'Are you sure you want to delete this income entry?'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.isLoading = true;
        this.incomeService.deleteIncome(id).subscribe({
          next: () => {
            this.loadIncomes();
          },
          error: (err) => {
            this.isLoading = false;
            this.errorMessage = err.error?.message || err.message || 'Failed to delete income record.';
          }
        });
      }
    });
  }
}
