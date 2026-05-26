import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { IncomeService } from '../services/income.service';
import { AccountService, Account } from '../../accounts/services/account.service';

@Component({
  selector: 'app-income-form',
  templateUrl: './income-form.component.html',
  styleUrls: ['./income-form.component.scss']
})
export class IncomeFormComponent implements OnInit {
  incomeForm!: FormGroup;
  isEditMode = false;
  incomeId: number | null = null;
  isLoading = false;
  accounts: Account[] = [];
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private incomeService: IncomeService,
    private accountService: AccountService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadAccounts();
    this.checkEditMode();
  }

  private initForm(): void {
    this.incomeForm = this.fb.group({
      accountId: ['', [Validators.required]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      source: ['', [Validators.required, Validators.maxLength(200)]],
      incomeDate: [new Date(), [Validators.required]]
    });
  }

  private loadAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (data) => this.accounts = data,
      error: () => console.error('Failed to load accounts')
    });
  }

  private checkEditMode(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.incomeId = Number(idParam);
      this.loadIncomeDetails(this.incomeId);
    }
  }

  private loadIncomeDetails(id: number): void {
    this.isLoading = true;
    this.incomeService.getIncomeById(id).subscribe({
      next: (income) => {
        this.incomeForm.patchValue({
          accountId: income.accountId,
          amount: income.amount,
          source: income.source,
          incomeDate: new Date(income.incomeDate)
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load income details.';
      }
    });
  }

  onSubmit(): void {
    if (this.incomeForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const formValue = this.incomeForm.value;
    const payload = {
      accountId: Number(formValue.accountId),
      amount: Number(formValue.amount),
      source: formValue.source.trim(),
      incomeDate: formValue.incomeDate instanceof Date ? formValue.incomeDate.toISOString() : new Date(formValue.incomeDate).toISOString()
    };

    if (this.isEditMode && this.incomeId) {
      this.incomeService.updateIncome(this.incomeId, payload).subscribe({
        next: () => this.router.navigate(['/income']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to update income record.';
        }
      });
    } else {
      this.incomeService.createIncome(payload).subscribe({
        next: () => this.router.navigate(['/income']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to record income.';
        }
      });
    }
  }
}
