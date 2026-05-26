import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-account-form',
  templateUrl: './account-form.component.html',
  styleUrls: ['./account-form.component.scss']
})
export class AccountFormComponent implements OnInit {
  accountForm!: FormGroup;
  isEditMode = false;
  accountId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  accountTypes = ['Bank', 'Credit Card', 'Cash'];

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.checkEditMode();
  }

  private initForm(): void {
    this.accountForm = this.fb.group({
      accountName: ['', [Validators.required, Validators.maxLength(100)]],
      accountType: ['Bank', [Validators.required]],
      accountNumber: ['', [Validators.maxLength(50)]],
      initialBalance: [0, [Validators.required, Validators.min(0)]]
    });
  }

  private checkEditMode(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.accountId = Number(idParam);
      this.loadAccountDetails(this.accountId);
    }
  }

  private loadAccountDetails(id: number): void {
    this.isLoading = true;
    this.accountService.getAccountById(id).subscribe({
      next: (account) => {
        this.accountForm.patchValue({
          accountName: account.accountName,
          accountType: account.accountType,
          accountNumber: account.accountNumber,
          initialBalance: account.initialBalance
        });
        // Initial balance is typically read-only during edit in standard finance apps
        this.accountForm.get('initialBalance')?.disable();
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load account details.';
      }
    });
  }

  onSubmit(): void {
    if (this.accountForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    // Get values, including disabled ones
    const formValue = this.accountForm.getRawValue();
    const payload = {
      accountName: formValue.accountName,
      accountType: formValue.accountType,
      accountNumber: formValue.accountNumber || null,
      initialBalance: Number(formValue.initialBalance)
    };

    if (this.isEditMode && this.accountId) {
      this.accountService.updateAccount(this.accountId, payload).subscribe({
        next: () => this.router.navigate(['/accounts']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to update account.';
        }
      });
    } else {
      this.accountService.createAccount(payload).subscribe({
        next: () => this.router.navigate(['/accounts']),
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.message || 'Failed to create account.';
        }
      });
    }
  }
}
