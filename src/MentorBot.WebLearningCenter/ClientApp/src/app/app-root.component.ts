import { Component, OnInit } from '@angular/core';
import { Chart, registerables } from 'chart.js';

const template = `<router-outlet></router-outlet>`;

const style = `
`;

@Component({
  selector: 'app-root,[app-root]',
  template: template,
  styles: [style],
})
export class AppRootComponent implements OnInit {
  ngOnInit(): void {
    Chart.register(...registerables);
  }
}
