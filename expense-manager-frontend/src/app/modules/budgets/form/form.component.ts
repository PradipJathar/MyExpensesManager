import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BudgetService } from '../services/budget.service';
import { CategoryService, Category } from '../../../core/services/category.service';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss']
})
export class FormComponent implements OnInit {
  budgetForm!: FormGroup;
  isEditMode = false;
  budgetId: number | null = null;
  isLoading = false;
  categories: Category[] = [];
  errorMessage: string | null = null;

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
    private fb: FormBuilder,
    private budgetService: BudgetService,
    private categoryService: CategoryService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear + 3; y++) {
      this.years.push(y);
    }
  }

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
    this.checkEditMode();
  }

  private initForm(): void {
    const currentDate = new Date();
    this.budgetForm = this.fb.group({
      categoryId: ['', [Validators.required]],
      budgetAmount: ['', [Validators.required, Validators.min(0.01)]],
      periodMonth: [currentDate.getMonth() + 1, [Validators.required]],
      periodYear: [currentDate.getFullYear(), [Validators.required]],
      alertThreshold: [90, [Validators.required, Validators.min(0), Validators.max(100)]]
    });
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => console.error('Failed to load categories')
    });
  }

  private checkEditMode(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.budgetId = Number(idParam);
      this.loadBudgetDetails(this.budgetId);
    }
  }

  private loadBudgetDetails(id: number): void {
    this.isLoading = true;
    this.budgetService.getBudgetById(id).subscribe({
      next: (budget) => {
        this.budgetForm.patchValue({
          categoryId: budget.categoryId,
          budgetAmount: budget.budgetAmount,
          periodMonth: budget.periodMonth,
          periodYear: budget.periodYear,
          alertThreshold: budget.alertThreshold
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load budget details.';
      }
    });
  }

  onSubmit(): void {
    if (this.budgetForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const formValue = {
      categoryId: Number(this.budgetForm.value.categoryId),
      budgetAmount: Number(this.budgetForm.value.budgetAmount),
      periodMonth: Number(this.budgetForm.value.periodMonth),
      periodYear: Number(this.budgetForm.value.periodYear),
      alertThreshold: Number(this.budgetForm.value.alertThreshold)
    };

    if (this.isEditMode && this.budgetId) {
      this.budgetService.updateBudget(this.budgetId, formValue).subscribe({
        next: () => this.router.navigate(['/budgets']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to update budget.';
        }
      });
    } else {
      this.budgetService.createBudget(formValue).subscribe({
        next: () => this.router.navigate(['/budgets']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to create budget.';
        }
      });
    }
  }
}

