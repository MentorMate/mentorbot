import { Component } from '@angular/core';

const template = `
  <nav>
    <a [routerLink]="['/app/dashboard']">Dashboard</a>
    <a [routerLink]="['/app/about']">About</a>
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
export class MenuComponent { }
