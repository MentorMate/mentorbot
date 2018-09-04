import { Component, OnInit } from '@angular/core';
import { AuthService } from './auth/auth.service';

const template = `
  <lp-menu></lp-menu>&nbsp;Hello {{name}} <a href="javascript:void(0);" (click)="signout();">signout</a>
  <hr />
  <router-outlet></router-outlet>
`;

const style = `
  :host {
    display: block;
    height: 100%;
    padding: 10px;
  }
`;

@Component({
  selector: 'lp-app',
  template: template,
  styles: [style]
})
export class AppMainComponent implements OnInit {
  name: string;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.name = this.authService.name;
  }

  signout(): void {
    this.authService.signout();
  }
}
