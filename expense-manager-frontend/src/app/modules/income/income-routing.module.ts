import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IncomeListComponent } from './list/income-list.component';
import { IncomeFormComponent } from './form/income-form.component';

const routes: Routes = [
  { path: '', component: IncomeListComponent },
  { path: 'new', component: IncomeFormComponent },
  { path: ':id/edit', component: IncomeFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IncomeRoutingModule { }
