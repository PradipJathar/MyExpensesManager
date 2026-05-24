import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { Expense } from '../models/expense.model';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  constructor(private apiService: ApiService) {}

  getAllExpenses(): Observable<Expense[]> {
    return this.apiService.get<Expense[]>('expenses');
  }

  getExpenseById(id: number): Observable<Expense> {
    return this.apiService.get<Expense>(`expenses/${id}`);
  }

  createExpense(expense: any): Observable<Expense> {
    return this.apiService.post<Expense>('expenses', expense);
  }

  updateExpense(id: number, expense: any): Observable<Expense> {
    return this.apiService.put<Expense>(`expenses/${id}`, expense);
  }

  deleteExpense(id: number): Observable<any> {
    return this.apiService.delete<any>(`expenses/${id}`);
  }

  filterExpenses(filters: {
    startDate?: string;
    endDate?: string;
    categoryId?: number;
    minAmount?: number;
    maxAmount?: number;
  }): Observable<Expense[]> {
    let params = new HttpParams();
    
    if (filters.startDate) {
      params = params.set('startDate', filters.startDate);
    }
    if (filters.endDate) {
      params = params.set('endDate', filters.endDate);
    }
    if (filters.categoryId) {
      params = params.set('categoryId', filters.categoryId.toString());
    }
    if (filters.minAmount) {
      params = params.set('minAmount', filters.minAmount.toString());
    }
    if (filters.maxAmount) {
      params = params.set('maxAmount', filters.maxAmount.toString());
    }

    return this.apiService.get<Expense[]>('expenses/filter', params);
  }
}
