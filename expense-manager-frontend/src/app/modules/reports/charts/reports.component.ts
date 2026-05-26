import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { Summary, CategoryBreakdown, Trend, MonthlyComparison, BudgetVsActual } from '../models/report.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  isLoading = true;
  errorMessage: string | null = null;
  filterForm!: FormGroup;

  // Key metrics
  summary!: Summary;

  // Chart configuration options
  public categoryPieOptions: any;
  public comparisonBarOptions: any;
  public trendsLineOptions: any;
  public budgetVsActualOptions: any;

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
    private fb: FormBuilder
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear + 3; y++) {
      this.years.push(y);
    }
  }

  ngOnInit(): void {
    this.initFilterForm();
    this.loadReportData();
  }

  private initFilterForm(): void {
    const currentDate = new Date();
    this.filterForm = this.fb.group({
      periodMonth: [currentDate.getMonth() + 1],
      periodYear: [currentDate.getFullYear()]
    });

    this.filterForm.valueChanges.subscribe(() => {
      this.loadReportData();
    });
  }

  loadReportData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    const filters = this.filterForm.value;
    const month = Number(filters.periodMonth);
    const year = Number(filters.periodYear);

    // Call API endpoints concurrently
    forkJoin({
      summary: this.reportService.getMonthlySummary(month, year),
      breakdown: this.reportService.getCategoryBreakdown(month, year),
      comparison: this.reportService.getMonthlyComparison(month, year),
      trends: this.reportService.getSpendingTrends(6),
      budgetVsActual: this.reportService.getBudgetVsActual(month, year)
    }).subscribe({
      next: (res) => {
        this.summary = res.summary;
        this.buildCategoryPieChart(res.breakdown);
        this.buildComparisonBarChart(res.comparison);
        this.buildTrendsLineChart(res.trends);
        this.buildBudgetVsActualChart(res.budgetVsActual);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Failed to load reports data.';
      }
    });
  }

  private buildCategoryPieChart(breakdown: CategoryBreakdown[]): void {
    if (breakdown.length === 0) {
      this.categoryPieOptions = null;
      return;
    }

    const series = breakdown.map(b => Number(b.amount));
    const labels = breakdown.map(b => b.categoryName);
    const colors = breakdown.map(b => b.colorCode || '#64748b');

    this.categoryPieOptions = {
      series: series,
      chart: {
        type: 'donut',
        height: 320,
        animations: { enabled: false }
      },
      labels: labels,
      colors: colors,
      legend: {
        position: 'bottom',
        fontFamily: 'Inter, sans-serif'
      },
      dataLabels: {
        enabled: true,
        formatter: (val: number) => `${val.toFixed(1)}%`
      },
      tooltip: {
        y: {
          formatter: (value: number) => `$${value.toFixed(2)}`
        }
      }
    };
  }

  private buildComparisonBarChart(comparison: MonthlyComparison): void {
    this.comparisonBarOptions = {
      series: [
        {
          name: 'Previous Month',
          data: [Number(comparison.previousMonthIncome), Number(comparison.previousMonthExpenses)]
        },
        {
          name: 'Current Month',
          data: [Number(comparison.currentMonthIncome), Number(comparison.currentMonthExpenses)]
        }
      ],
      chart: {
        type: 'bar',
        height: 320,
        animations: { enabled: false },
        toolbar: { show: false }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '55%',
          borderRadius: 4
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        show: true,
        width: 2,
        colors: ['transparent']
      },
      xaxis: {
        categories: ['Income', 'Expenses'],
        labels: {
          style: {
            colors: '#64748b',
            fontFamily: 'Inter, sans-serif'
          }
        }
      },
      yaxis: {
        title: {
          text: 'Amount ($)',
          style: {
            color: '#64748b',
            fontFamily: 'Inter, sans-serif'
          }
        },
        labels: {
          formatter: (val: number) => `$${val.toFixed(0)}`
        }
      },
      fill: {
        opacity: 1
      },
      colors: ['#94a3b8', '#3b82f6'], // light blue/gray, vivid blue
      legend: {
        position: 'bottom',
        fontFamily: 'Inter, sans-serif'
      },
      tooltip: {
        y: {
          formatter: (val: number) => `$${val.toFixed(2)}`
        }
      }
    };
  }

  private buildTrendsLineChart(trends: Trend[]): void {
    const sortedTrends = [...trends].reverse();
    const categories = sortedTrends.map(t => t.label);
    const incomeData = sortedTrends.map(t => t.income);
    const expenseData = sortedTrends.map(t => t.expense);

    this.trendsLineOptions = {
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
        type: 'line',
        height: 320,
        animations: { enabled: false },
        toolbar: { show: false }
      },
      colors: ['#10b981', '#ef4444'], // green, red
      stroke: {
        width: 3,
        curve: 'smooth'
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
          formatter: (val: number) => `$${val.toFixed(0)}`
        }
      },
      legend: {
        position: 'bottom',
        fontFamily: 'Inter, sans-serif'
      },
      tooltip: {
        y: {
          formatter: (val: number) => `$${val.toFixed(2)}`
        }
      }
    };
  }

  private buildBudgetVsActualChart(budgetVsActual: BudgetVsActual[]): void {
    if (budgetVsActual.length === 0) {
      this.budgetVsActualOptions = null;
      return;
    }

    const categories = budgetVsActual.map(b => b.categoryName);
    const budgetData = budgetVsActual.map(b => Number(b.budgetAmount));
    const actualData = budgetVsActual.map(b => Number(b.spentAmount));

    this.budgetVsActualOptions = {
      series: [
        {
          name: 'Budget Limit',
          data: budgetData
        },
        {
          name: 'Actual Spent',
          data: actualData
        }
      ],
      chart: {
        type: 'bar',
        height: 320,
        animations: { enabled: false },
        toolbar: { show: false }
      },
      plotOptions: {
        bar: {
          horizontal: true,
          dataLabels: {
            position: 'top'
          }
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        show: true,
        width: 1,
        colors: ['#fff']
      },
      xaxis: {
        categories: categories,
        labels: {
          formatter: (val: number) => `$${val.toFixed(0)}`
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#64748b',
            fontFamily: 'Inter, sans-serif'
          }
        }
      },
      colors: ['#cbd5e1', '#f59e0b'], // slate-300, amber-500
      legend: {
        position: 'bottom',
        fontFamily: 'Inter, sans-serif'
      },
      tooltip: {
        y: {
          formatter: (val: number) => `$${val.toFixed(2)}`
        }
      }
    };
  }
}
