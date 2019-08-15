import { Component } from '@angular/core';

const template = `
  Page Not Found
`;

const style = `
  :host {
    display: block;
  }
`;

@Component({
  selector: 'app-not-found',
  template: template,
  styles: [style]
})
export class NotFoundPageComponent { }
