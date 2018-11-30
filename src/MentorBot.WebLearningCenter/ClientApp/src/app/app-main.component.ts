import { Component } from '@angular/core';

const template = `
  <lp-menu></lp-menu>
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
export class AppMainComponent {
}
