import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ListComponent } from './list/list.component';
import { FormComponent } from './form/form.component';
import { DetailComponent } from './detail/detail.component';

const routes: Routes = [
  { path: '', component: ListComponent },
  { path: 'new', component: FormComponent },
  { path: ':id/edit', component: FormComponent },
  { path: ':id', component: DetailComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExpensesRoutingModule { }

