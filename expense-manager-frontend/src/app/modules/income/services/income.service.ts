import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

export interface Income {
  id: number;
  accountId: number;
  accountName: string;
  amount: number;
  source: string;
  incomeDate: string;
  createdAt: string;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class IncomeService {
  constructor(private apiService: ApiService) {}

  getAllIncome(): Observable<Income[]> {
    return this.apiService.get<Income[]>('income');
  }

  getIncomeById(id: number): Observable<Income> {
    return this.apiService.get<Income>(`income/${id}`);
  }

  createIncome(income: any): Observable<Income> {
    return this.apiService.post<Income>('income', income);
  }

  updateIncome(id: number, income: any): Observable<Income> {
    return this.apiService.put<Income>(`income/${id}`, income);
  }

  deleteIncome(id: number): Observable<any> {
    return this.apiService.delete<any>(`income/${id}`);
  }

  getMonthlyIncome(month: number, year: number): Observable<number> {
    return this.apiService.get<number>(`income/monthly/${month}/${year}`);
  }
}
