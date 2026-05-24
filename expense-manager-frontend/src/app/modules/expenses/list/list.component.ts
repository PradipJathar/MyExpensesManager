import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { ExpenseService } from '../services/expense.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { Expense } from '../models/expense.model';
import { DeleteConfirmDialogComponent } from '../../../shared/components/delete-confirm-dialog/delete-confirm-dialog.component';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, AfterViewInit {
  dataSource = new MatTableDataSource<Expense>();
  displayedColumns: string[] = ['expenseDate', 'category', 'description', 'amount', 'account', 'actions'];
  isLoading = true;
  categories: Category[] = [];
  filterForm!: FormGroup;
  errorMessage: string | null = null;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private expenseService: ExpenseService,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.initFilterForm();
    this.loadCategories();
    this.loadExpenses();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      startDate: [''],
      endDate: [''],
      categoryId: [''],
      minAmount: [''],
      maxAmount: ['']
    });
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => console.error('Failed to load categories')
    });
  }

  loadExpenses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.expenseService.getAllExpenses().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load expenses.';
      }
    });
  }

  applyFilters(): void {
    const rawFilters = this.filterForm.value;
    const filters: any = {};

    if (rawFilters.startDate) {
      filters.startDate = new Date(rawFilters.startDate).toISOString();
    }
    if (rawFilters.endDate) {
      filters.endDate = new Date(rawFilters.endDate).toISOString();
    }
    if (rawFilters.categoryId) {
      filters.categoryId = Number(rawFilters.categoryId);
    }
    if (rawFilters.minAmount) {
      filters.minAmount = Number(rawFilters.minAmount);
    }
    if (rawFilters.maxAmount) {
      filters.maxAmount = Number(rawFilters.maxAmount);
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.expenseService.filterExpenses(filters).subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to filter expenses.';
      }
    });
  }

  resetFilters(): void {
    this.filterForm.reset();
    this.loadExpenses();
  }

  onDelete(id: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Expense',
        message: 'Are you sure you want to delete this expense record?'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.isLoading = true;
        this.expenseService.deleteExpense(id).subscribe({
          next: () => {
            this.loadExpenses();
          },
          error: (err) => {
            this.isLoading = false;
            this.errorMessage = err.message || 'Failed to delete expense.';
          }
        });
      }
    });
  }
}
