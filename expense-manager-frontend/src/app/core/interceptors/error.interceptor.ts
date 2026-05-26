import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unexpected error occurred.';

        if (error.error instanceof ErrorEvent) {
          // Client-side or network error
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Server-side error
          if (error.status === 401) {
            // Unauthorized - auto logout and redirect
            this.authService.logout();
            this.router.navigate(['/auth/login']);
            errorMessage = 'Session expired. Please log in again.';
          } else if (error.error && error.error.message) {
            errorMessage = error.error.message;
          } else if (error.error && typeof error.error === 'string') {
            errorMessage = error.error;
          } else {
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
          }
        }

        // Show snackbar message
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: error.status === 401 ? ['warn-snackbar'] : ['error-snackbar']
        });

        return throwError(() => new Error(errorMessage));
      })
    );
  }
}
