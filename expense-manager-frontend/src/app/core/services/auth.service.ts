import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser$: Observable<User | null>;

  private readonly TOKEN_KEY = 'em_access_token';
  private readonly REFRESH_TOKEN_KEY = 'em_refresh_token';
  private readonly USER_KEY = 'em_current_user';

  constructor(private apiService: ApiService) {
    const savedUser = localStorage.getItem(this.USER_KEY);
    this.currentUserSubject = new BehaviorSubject<User | null>(
      savedUser ? JSON.parse(savedUser) : null
    );
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public setSession(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
    this.currentUserSubject.next(authResponse.user);
  }

  public register(email: string, password: string, firstName: string, lastName: string): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('auth/register', { email, password, firstName, lastName })
      .pipe(
        map((response: AuthResponse) => {
          this.setSession(response);
          return response;
        })
      );
  }

  public login(email: string, password: string): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('auth/login', { email, password })
      .pipe(
        map((response: AuthResponse) => {
          this.setSession(response);
          return response;
        })
      );
  }

  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
  }

  public changePassword(oldPassword: string, newPassword: string): Observable<any> {
    return this.apiService.post('auth/change-password', { oldPassword, newPassword });
  }

  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  public getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  public isLoggedIn(): boolean {
    return this.getToken() !== null;
  }
}
