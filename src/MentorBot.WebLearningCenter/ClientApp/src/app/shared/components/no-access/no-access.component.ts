import { Component } from '@angular/core';

const template = `
  You do not have access to this page!
`;

const style = `
  :host {
    display: block;
  }
`;

@Component({
  selector: 'app-no-access',
  template: template,
  styles: [style],
})
export class NoAccessPageComponent {}
