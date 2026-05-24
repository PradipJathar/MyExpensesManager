import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ExpenseService } from '../services/expense.service';
import { Expense } from '../models/expense.model';
import { DeleteConfirmDialogComponent } from '../../../shared/components/delete-confirm-dialog/delete-confirm-dialog.component';

@Component({
  selector: 'app-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.scss']
})
export class DetailComponent implements OnInit {
  expense: Expense | null = null;
  isLoading = true;
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private expenseService: ExpenseService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.loadExpense(Number(idParam));
    } else {
      this.router.navigate(['/expenses']);
    }
  }

  private loadExpense(id: number): void {
    this.isLoading = true;
    this.expenseService.getExpenseById(id).subscribe({
      next: (data) => {
        this.expense = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load expense details.';
      }
    });
  }

  onDelete(): void {
    if (!this.expense) return;

    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Expense',
        message: 'Are you sure you want to delete this expense record?'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed && this.expense) {
        this.isLoading = true;
        this.expenseService.deleteExpense(this.expense.id).subscribe({
          next: () => {
            this.router.navigate(['/expenses']);
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
