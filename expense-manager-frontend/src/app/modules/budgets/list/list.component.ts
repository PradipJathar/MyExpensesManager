import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BudgetService } from '../services/budget.service';
import { BudgetStatus } from '../models/budget.model';
import { DeleteConfirmDialogComponent } from '../../../shared/components/delete-confirm-dialog/delete-confirm-dialog.component';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  allBudgets: BudgetStatus[] = [];
  filteredBudgets: BudgetStatus[] = [];
  isLoading = true;
  errorMessage: string | null = null;
  filterForm!: FormGroup;

  totalBudgeted = 0;
  totalSpent = 0;
  totalRemaining = 0;
  totalPercentage = 0;

  months = [
    { value: 1, name: 'January' },
    { value: 2, name: 'February' },
    { value: 3, name: 'March' },
    { value: 4, name: 'April' },
    { value: 5, name: 'May' },
    { value: 6, name: 'June' },
    { value: 7, name: 'July' },
    { value: 8, name: 'August' },
    { value: 9, name: 'September' },
    { value: 10, name: 'October' },
    { value: 11, name: 'November' },
    { value: 12, name: 'December' }
  ];

  years: number[] = [];

  constructor(
    private budgetService: BudgetService,
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear + 3; y++) {
      this.years.push(y);
    }
  }

  ngOnInit(): void {
    this.initFilterForm();
    this.loadBudgetStatuses();
  }

  private initFilterForm(): void {
    const currentDate = new Date();
    this.filterForm = this.fb.group({
      periodMonth: [currentDate.getMonth() + 1],
      periodYear: [currentDate.getFullYear()]
    });

    this.filterForm.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  loadBudgetStatuses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.budgetService.getBudgetStatus().subscribe({
      next: (data) => {
        this.allBudgets = data;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load budgets.';
      }
    });
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    
    this.filteredBudgets = this.allBudgets.filter(b => {
      let matches = true;
      if (filters.periodMonth) {
        matches = matches && b.periodMonth === Number(filters.periodMonth);
      }
      if (filters.periodYear) {
        matches = matches && b.periodYear === Number(filters.periodYear);
      }
      return matches;
    });

    this.calculateTotals();
  }

  private calculateTotals(): void {
    this.totalBudgeted = this.filteredBudgets.reduce((sum, b) => sum + b.budgetAmount, 0);
    this.totalSpent = this.filteredBudgets.reduce((sum, b) => sum + b.spentAmount, 0);
    this.totalRemaining = this.totalBudgeted - this.totalSpent;
    this.totalPercentage = this.totalBudgeted > 0 ? (this.totalSpent / this.totalBudgeted) * 100 : 0;
  }

  resetFilters(): void {
    const currentDate = new Date();
    this.filterForm.patchValue({
      periodMonth: currentDate.getMonth() + 1,
      periodYear: currentDate.getFullYear()
    });
  }

  getProgressBarClass(percentage: number): string {
    if (percentage < 70) return 'progress-green';
    if (percentage < 90) return 'progress-yellow';
    if (percentage <= 100) return 'progress-red';
    return 'progress-dark-red';
  }

  getBoundedPercentage(percentage: number): number {
    return Math.min(percentage, 100);
  }

  onDelete(id: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Budget',
        message: 'Are you sure you want to delete this category budget?'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.isLoading = true;
        this.budgetService.deleteBudget(id).subscribe({
          next: () => {
            this.loadBudgetStatuses();
          },
          error: (err) => {
            this.isLoading = false;
            this.errorMessage = err.message || 'Failed to delete budget.';
          }
        });
      }
    });
  }
}

