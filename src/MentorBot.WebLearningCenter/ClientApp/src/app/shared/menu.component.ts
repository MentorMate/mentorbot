import { Component, OnInit } from '@angular/core';

import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';

const template = `
  <nav class="navbar navbar-default">
    <div class="container-fluid">
      <div class="navbar-header">
        <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
          <span class="sr-only">Toggle navigation</span>
          <span class="icon-bar"></span>
          <span class="icon-bar"></span>
          <span class="icon-bar"></span>
        </button>
      </div>
      <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
        <ul class="nav navbar-nav">
          <li><a routerLink="/app/dashboard" routerLinkActive="active">Dashboard</a></li>
          <li><a routerLink="/app/settings" routerLinkActive="active">Settings</a></li>
          <li><a routerLink="/app/about" routerLinkActive="active">About</a></li>
        </ul>
        <ul class="nav navbar-nav navbar-right">
          <li><p class="navbar-text">Signed in as {{name}}</p></li>
          <li><button type="button" class="btn btn-default navbar-btn" (click)="signout()">Sign out</button></li>
        </ul>
      </div>
    </div>
  </nav>
`;

const style = `
  :host {
    display: block;
  }
`;

@Component({
  selector: 'lp-menu',
  template: template,
  styles: [style]
})
export class MenuComponent implements OnInit {
  name: string;

  constructor(
    private authService: AuthService,
    private router: Router) { }

  ngOnInit() {
    this.name = this.authService.name;
  }

  signout(): void {
    this.authService.signout();
    setTimeout(() => this.router.navigate(['/', 'app', 'signout-callback']), 300);
  }
}
