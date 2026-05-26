import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { Budget, BudgetStatus } from '../models/budget.model';

@Injectable({
  providedIn: 'root'
})
export class BudgetService {
  constructor(private apiService: ApiService) {}

  getAllBudgets(): Observable<Budget[]> {
    return this.apiService.get<Budget[]>('budgets');
  }

  getBudgetById(id: number): Observable<Budget> {
    return this.apiService.get<Budget>(`budgets/${id}`);
  }

  createBudget(budget: any): Observable<Budget> {
    return this.apiService.post<Budget>('budgets', budget);
  }

  updateBudget(id: number, budget: any): Observable<Budget> {
    return this.apiService.put<Budget>(`budgets/${id}`, budget);
  }

  deleteBudget(id: number): Observable<any> {
    return this.apiService.delete<any>(`budgets/${id}`);
  }

  getBudgetStatus(): Observable<BudgetStatus[]> {
    return this.apiService.get<BudgetStatus[]>('budgets/status');
  }
}
