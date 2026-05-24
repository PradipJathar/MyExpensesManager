import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Account {
  id: number;
  accountName: string;
  accountType: string;
  accountNumber?: string;
  initialBalance: number;
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
}
