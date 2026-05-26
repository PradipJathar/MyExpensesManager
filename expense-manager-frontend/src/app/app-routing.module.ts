import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { NotFoundComponent } from './shared/components/not-found/not-found.component';

const routes: Routes = [
  { path: 'auth', loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule) },
  { path: 'dashboard', loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule), canActivate: [AuthGuard] },
  { path: 'expenses', loadChildren: () => import('./modules/expenses/expenses.module').then(m => m.ExpensesModule), canActivate: [AuthGuard] },
  { path: 'income', loadChildren: () => import('./modules/income/income.module').then(m => m.IncomeModule), canActivate: [AuthGuard] },
  { path: 'budgets', loadChildren: () => import('./modules/budgets/budgets.module').then(m => m.BudgetsModule), canActivate: [AuthGuard] },
  { path: 'accounts', loadChildren: () => import('./modules/accounts/accounts.module').then(m => m.AccountsModule), canActivate: [AuthGuard] },
  { path: 'reports', loadChildren: () => import('./modules/reports/reports.module').then(m => m.ReportsModule), canActivate: [AuthGuard] },
  { path: 'settings', loadChildren: () => import('./modules/settings/settings.module').then(m => m.SettingsModule), canActivate: [AuthGuard] },
  { path: '404', component: NotFoundComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '404' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
