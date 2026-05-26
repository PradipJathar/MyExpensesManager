import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

export interface Account {
  id: number;
  accountName: string;
  accountType: string;
  accountNumber?: string;
  initialBalance: number;
  currentBalance: number;
  createdAt: string;
  updatedAt: string;
}

export interface AccountBalanceResponse {
  accountId: number;
  currentBalance: number;
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private apiService: ApiService) {}

  getAllAccounts(): Observable<Account[]> {
    return this.apiService.get<Account[]>('accounts');
  }

  getAccountById(id: number): Observable<Account> {
    return this.apiService.get<Account>(`accounts/${id}`);
  }

  createAccount(account: any): Observable<Account> {
    return this.apiService.post<Account>('accounts', account);
  }

  updateAccount(id: number, account: any): Observable<Account> {
    return this.apiService.put<Account>(`accounts/${id}`, account);
  }

  deleteAccount(id: number): Observable<any> {
    return this.apiService.delete<any>(`accounts/${id}`);
  }

  getAccountBalance(id: number): Observable<AccountBalanceResponse> {
    return this.apiService.get<AccountBalanceResponse>(`accounts/${id}/balance`);
  }
}
