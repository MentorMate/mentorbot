import { Component, OnInit } from '@angular/core';

import { AuthService } from '../../../auth/auth.service';
import { Router } from '@angular/router';
import { checkPath } from '../../../auth/auth.operations';

const style = `
  :host {
    display: block;
  }
`;

@Component({
  selector: 'app-menu',
  templateUrl: 'menu.component.html',
  styles: [style],
})
export class MenuComponent implements OnInit {
  name?: string;
  paths = [
    { url: '/app/dashboard', name: 'Dashboard' },
    { url: '/app/settings', name: 'Settings' },
    { url: '/app/users', name: 'Users' },
    { url: '/app/about', name: 'About' },
    { url: '/app/questions', name: 'Questions' },
  ];

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.name = this.authService.name;
  }

  allow(url: string): boolean {
    return checkPath(url);
  }

  signout(): void {
    this.authService.signout();
    setTimeout(() => this.router.navigate(['/', 'app', 'signout-callback']), 300);
  }
}
