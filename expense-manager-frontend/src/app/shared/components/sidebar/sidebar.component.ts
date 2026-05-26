import { Component, Input, OnInit } from '@angular/core';

interface MenuItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Input() isCollapsed = false;

  menuItems: MenuItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Expenses', route: '/expenses', icon: 'receipt' },
    { label: 'Income', route: '/income', icon: 'trending_up' },
    { label: 'Budgets', route: '/budgets', icon: 'pie_chart' },
    { label: 'Accounts', route: '/accounts', icon: 'account_balance_wallet' },
    { label: 'Reports', route: '/reports', icon: 'bar_chart' },
    { label: 'Settings', route: '/settings/profile', icon: 'settings' }
  ];

  constructor() {}

  ngOnInit(): void {}
}
