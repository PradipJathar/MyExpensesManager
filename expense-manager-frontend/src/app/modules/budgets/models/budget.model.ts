export interface Budget {
  id: number;
  categoryId: number;
  categoryName: string;
  budgetAmount: number;
  periodMonth: number;
  periodYear: number;
  alertThreshold: number;
  createdAt: string;
  updatedAt: string;
}

export interface BudgetStatus {
  budgetId: number;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  budgetAmount: number;
  spentAmount: number;
  periodMonth: number;
  periodYear: number;
  percentageUsed: number;
  isExceeded: boolean;
  isAlert: boolean;
}
