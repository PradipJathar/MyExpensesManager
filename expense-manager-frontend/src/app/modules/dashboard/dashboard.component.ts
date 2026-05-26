import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportService } from '../reports/services/report.service';
import { ExpenseService } from '../expenses/services/expense.service';
import { Summary, Trend } from '../reports/models/report.model';
import { Expense } from '../expenses/models/expense.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  isLoading = true;
  errorMessage: string | null = null;
  
  summary!: Summary;
  recentExpenses: Expense[] = [];
  filterForm!: FormGroup;

  // Chart setup
  public chartOptions: any;

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
    private reportService: ReportService,
    private expenseService: ExpenseService,
    private fb: FormBuilder
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear + 3; y++) {
      this.years.push(y);
    }
  }

  ngOnInit(): void {
    this.initFilterForm();
    this.loadDashboardData();
  }

  private initFilterForm(): void {
    const currentDate = new Date();
    this.filterForm = this.fb.group({
      periodMonth: [currentDate.getMonth() + 1],
      periodYear: [currentDate.getFullYear()]
    });

    this.filterForm.valueChanges.subscribe(() => {
      this.loadDashboardData();
    });
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    const filters = this.filterForm.value;
    const month = Number(filters.periodMonth);
    const year = Number(filters.periodYear);

    // Run parallel calls to API
    forkJoin({
      summary: this.reportService.getMonthlySummary(month, year),
      expenses: this.expenseService.getAllExpenses(),
      trends: this.reportService.getSpendingTrends(6)
    }).subscribe({
      next: (res) => {
        this.summary = res.summary;
        
        // Filter expenses for selected month and year, sort descending by date, and take top 5
        this.recentExpenses = res.expenses
          .filter(e => {
            const expDate = new Date(e.expenseDate);
            return (expDate.getMonth() + 1) === month && expDate.getFullYear() === year;
          })
          .sort((a, b) => new Date(b.expenseDate).getTime() - new Date(a.expenseDate).getTime())
          .slice(0, 5);

        this.initTrendChart(res.trends);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load dashboard data.';
      }
    });
  }

  private initTrendChart(trends: Trend[]): void {
    // Reverse to show chronological order (oldest to newest)
    const sortedTrends = [...trends].reverse();
    const categories = sortedTrends.map(t => t.label);
    const incomeData = sortedTrends.map(t => t.income);
    const expenseData = sortedTrends.map(t => t.expense);

    this.chartOptions = {
      series: [
        {
          name: 'Income',
          data: incomeData
        },
        {
          name: 'Expenses',
          data: expenseData
        }
      ],
      chart: {
        height: 240,
        type: 'area',
        toolbar: {
          show: false
        },
        animations: {
          enabled: false
        }
      },
      colors: ['#10b981', '#ef4444'], // green, red
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: 'smooth',
        width: 3
      },
      fill: {
        type: 'gradient',
        gradient: {
          opacityFrom: 0.4,
          opacityTo: 0.1,
        }
      },
      xaxis: {
        categories: categories,
        labels: {
          style: {
            colors: '#64748b',
            fontFamily: 'Inter, sans-serif'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#64748b',
            fontFamily: 'Inter, sans-serif'
          },
          formatter: (value: number) => `$${value.toFixed(0)}`
        }
      },
      tooltip: {
        x: {
          format: 'dd/MM/yy HH:mm'
        },
        theme: 'light'
      },
      legend: {
        position: 'bottom',
        fontFamily: 'Inter, sans-serif',
        labels: {
          colors: '#475569'
        }
      }
    };
  }

  getBudgetPercentage(spent: number, budgeted: number): number {
    if (budgeted <= 0) return 0;
    return Math.min((spent / budgeted) * 100, 100);
  }
}
