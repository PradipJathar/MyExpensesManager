import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { Summary, CategoryBreakdown, Trend, MonthlyComparison, BudgetVsActual } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  constructor(private apiService: ApiService) {}

  getMonthlySummary(month: number, year: number): Observable<Summary> {
    return this.apiService.get<Summary>(`reports/summary/${month}/${year}`);
  }

  getYearlySummary(year: number): Observable<Trend[]> {
    return this.apiService.get<Trend[]>(`reports/yearly/${year}`);
  }

  getCategoryBreakdown(month: number, year: number): Observable<CategoryBreakdown[]> {
    return this.apiService.get<CategoryBreakdown[]>(`reports/category-breakdown/${month}/${year}`);
  }

  getMonthlyComparison(month: number, year: number): Observable<MonthlyComparison> {
    return this.apiService.get<MonthlyComparison>(`reports/monthly-comparison/${month}/${year}`);
  }

  getSpendingTrends(months: number = 6): Observable<Trend[]> {
    return this.apiService.get<Trend[]>(`reports/trends?months=${months}`);
  }

  getBudgetVsActual(month: number, year: number): Observable<BudgetVsActual[]> {
    return this.apiService.get<BudgetVsActual[]>(`reports/budget-vs-actual/${month}/${year}`);
  }
}
