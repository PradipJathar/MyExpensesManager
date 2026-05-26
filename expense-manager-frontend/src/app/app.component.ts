import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, User } from './core/services/auth.service';
import { LoadingService } from './core/services/loading.service';
import { Observable, Subscription } from 'rxjs';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, shareReplay } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  currentUser$: Observable<User | null>;
  isLoading$: Observable<boolean>;
  isSidebarCollapsed = false;
  isHandset = false;

  isHandset$: Observable<boolean>;
  private handsetSub!: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    public loadingService: LoadingService,
    private breakpointObserver: BreakpointObserver
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.isLoading$ = this.loadingService.isLoading$;

    this.isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
      .pipe(
        map(result => result.matches),
        shareReplay()
      );
  }

  ngOnInit(): void {
    this.handsetSub = this.isHandset$.subscribe(val => {
      this.isHandset = val;
    });
  }

  ngOnDestroy(): void {
    if (this.handsetSub) {
      this.handsetSub.unsubscribe();
    }
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
