import { AfterContentInit, Directive, ElementRef, Input, NgZone, OnChanges, SimpleChange, SimpleChanges } from '@angular/core';

import { Chart, ChartDataset } from 'chart.js';

@Directive({
  selector: 'canvas[appChart][appChartDatasets]',
})
export class ChartDirective implements AfterContentInit, OnChanges {
  @Input() appChart: 'pie' | 'line' | 'bar' | 'radar' | 'polarArea' | 'bubble' = 'pie';
  @Input() appChartLabels?: string[];
  @Input() appChartDatasets: ChartDataset[] = [];

  private _chart: Chart | null = null;

  constructor(private readonly _element: ElementRef, private readonly _ngZone: NgZone) {}

  ngAfterContentInit(): void {
    this._ngZone.runOutsideAngular(() => {
      if (this._chart !== null) {
        this._chart.destroy();
      }

      const element = this._element.nativeElement as HTMLCanvasElement;
      const ctx = element.getContext('2d');
      if (ctx === null) {
        throw new Error('No 2d context found');
      }

      this._chart = new Chart(ctx, {
        type: this.appChart,
        data: {
          labels: this.appChartLabels || [],
          datasets: !this.appChartDatasets || !Array.isArray(this.appChartDatasets) ? [] : this.appChartDatasets,
        },
        options: {
          animation: {
            duration: 800,
            easing: 'easeOutBounce',
          },
        },
      });
    })
    
  }

  ngOnChanges(changes: SimpleChanges): void {
    const appChartDatasets: SimpleChange | undefined = changes['appChartDatasets'];
    if (appChartDatasets && !appChartDatasets.isFirstChange && appChartDatasets.currentValue !== null) {
      this._ngZone.runOutsideAngular(() => {
        if (this._chart !== null) {
          this._chart.data.datasets = appChartDatasets.currentValue;
          this._chart.update();
        }
      });
    }
  }
}
