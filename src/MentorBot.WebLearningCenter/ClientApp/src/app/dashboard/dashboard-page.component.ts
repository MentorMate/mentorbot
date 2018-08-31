import { Component } from '@angular/core';

const template = `
  Dashboard
`;

const style = `
  :host {
    display: block;
  }
`;

@Component({
  selector: 'lp-dashboard',
  template: template,
  styles: [style]
})
export class DashboardPageComponent { }
