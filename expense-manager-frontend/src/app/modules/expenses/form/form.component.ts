import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ExpenseService } from '../services/expense.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { AccountService, Account } from '../../../core/services/account.service';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss']
})
export class FormComponent implements OnInit {
  expenseForm!: FormGroup;
  isEditMode = false;
  expenseId: number | null = null;
  isLoading = false;
  categories: Category[] = [];
  accounts: Account[] = [];
  errorMessage: string | null = null;
  maxDate = new Date(); // Enforce date not in future

  constructor(
    private fb: FormBuilder,
    private expenseService: ExpenseService,
    private categoryService: CategoryService,
    private accountService: AccountService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadDropdowns();
    this.checkEditMode();
  }

  private initForm(): void {
    this.expenseForm = this.fb.group({
      categoryId: ['', [Validators.required]],
      accountId: ['', [Validators.required]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      description: ['', [Validators.maxLength(500)]],
      expenseDate: [new Date(), [Validators.required]]
    });
  }

  private loadDropdowns(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => console.error('Failed to load categories')
    });

    this.accountService.getAllAccounts().subscribe({
      next: (data) => this.accounts = data,
      error: () => console.error('Failed to load accounts')
    });
  }

  private checkEditMode(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.expenseId = Number(idParam);
      this.loadExpenseDetails(this.expenseId);
    }
  }

  private loadExpenseDetails(id: number): void {
    this.isLoading = true;
    this.expenseService.getExpenseById(id).subscribe({
      next: (expense) => {
        this.expenseForm.patchValue({
          categoryId: expense.categoryId,
          accountId: expense.accountId,
          amount: expense.amount,
          description: expense.description,
          expenseDate: new Date(expense.expenseDate)
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load expense details.';
      }
    });
  }

  onSubmit(): void {
    if (this.expenseForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const formValue = {
      ...this.expenseForm.value,
      expenseDate: new Date(this.expenseForm.value.expenseDate).toISOString()
    };

    if (this.isEditMode && this.expenseId) {
      this.expenseService.updateExpense(this.expenseId, formValue).subscribe({
        next: () => this.router.navigate(['/expenses']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.message || 'Failed to update expense.';
        }
      });
    } else {
      this.expenseService.createExpense(formValue).subscribe({
        next: () => this.router.navigate(['/expenses']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.message || 'Failed to create expense.';
        }
      });
    }
  }
}
