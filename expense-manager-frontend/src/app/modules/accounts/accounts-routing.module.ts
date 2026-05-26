import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountListComponent } from './list/account-list.component';
import { AccountFormComponent } from './form/account-form.component';

const routes: Routes = [
  { path: '', component: AccountListComponent },
  { path: 'new', component: AccountFormComponent },
  { path: ':id/edit', component: AccountFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountsRoutingModule { }
