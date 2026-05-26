import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AccountService, Account } from '../services/account.service';
import { DeleteConfirmDialogComponent } from '../../../shared/components/delete-confirm-dialog/delete-confirm-dialog.component';

@Component({
  selector: 'app-account-list',
  templateUrl: './account-list.component.html',
  styleUrls: ['./account-list.component.scss']
})
export class AccountListComponent implements OnInit {
  accounts: Account[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  totalAssets = 0;
  totalLiabilities = 0;
  netWealth = 0;

  constructor(
    private accountService: AccountService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.accountService.getAllAccounts().subscribe({
      next: (data) => {
        this.accounts = data;
        this.calculateTotals();
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load accounts.';
      }
    });
  }

  private calculateTotals(): void {
    this.totalAssets = this.accounts
      .filter(a => a.accountType === 'Bank' || a.accountType === 'Cash')
      .reduce((sum, a) => sum + a.currentBalance, 0);

    this.totalLiabilities = this.accounts
      .filter(a => a.accountType === 'Credit Card')
      .reduce((sum, a) => sum + a.currentBalance, 0);

    this.netWealth = this.totalAssets - this.totalLiabilities;
  }

  getAccountTypeIcon(type: string): string {
    switch (type) {
      case 'Bank': return 'account_balance';
      case 'Credit Card': return 'credit_card';
      case 'Cash': return 'payments';
      default: return 'wallet';
    }
  }

  onDelete(id: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Account',
        message: 'Are you sure you want to delete this account? All associated transaction histories will be lost.'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.isLoading = true;
        this.accountService.deleteAccount(id).subscribe({
          next: () => {
            this.loadAccounts();
          },
          error: (err) => {
            this.isLoading = false;
            this.errorMessage = err.error?.message || err.message || 'Failed to delete account.';
          }
        });
      }
    });
  }
}
