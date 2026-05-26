export interface Summary {
  totalIncome: number;
  totalExpenses: number;
  netAmount: number;
  totalBudgeted: number;
  totalBudgetSpent: number;
}

export interface CategoryBreakdown {
  categoryName: string;
  colorCode: string;
  amount: number;
  percentage: number;
}

export interface Trend {
  label: string;
  month: number;
  year: number;
  income: number;
  expense: number;
  net: number;
}

export interface MonthlyComparison {
  currentMonthExpenses: number;
  previousMonthExpenses: number;
  expenseChangePercentage: number;
  currentMonthIncome: number;
  previousMonthIncome: number;
  incomeChangePercentage: number;
}

export interface BudgetVsActual {
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  budgetAmount: number;
  spentAmount: number;
  percentageUsed: number;
  isExceeded: boolean;
}
