import { Component } from '@angular/core';

const template = `
  <app-menu></app-menu>
  <router-outlet></router-outlet>
`;

const style = `
  :host {
    display: block;
    height: 100%;
    padding: 10px;
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
  }
`;

@Component({
  selector: 'app-main',
  template: template,
  styles: [style],
})
export class AppMainComponent {}
