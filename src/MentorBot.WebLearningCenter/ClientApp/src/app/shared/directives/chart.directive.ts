import { AfterContentInit, Directive, ElementRef, Input, NgZone, OnChanges, SimpleChanges } from '@angular/core';
import { Chart, ChartDataSets } from 'chart.js';

@Directive({
  selector: 'canvas[appChart][appChartDatasets]'
})
export class ChartDirective implements AfterContentInit, OnChanges {
  @Input() appChart: 'pie' | 'line' | 'bar' | 'radar' | 'polarArea' | 'bubble';
  @Input() appChartLabels: string[];
  @Input() appChartDatasets: ChartDataSets[];

  private _chart: Chart = null;

  constructor(private readonly _element: ElementRef) {
  }

  ngAfterContentInit(): void {
    if (this._chart !== null) {
      this._chart.destroy();
    }

    this._chart = new Chart(this._element.nativeElement, {
      type: this.appChart,
      data: {
        labels: this.appChartLabels || [],
        datasets: !this.appChartDatasets || !Array.isArray(this.appChartDatasets) ? [] : this.appChartDatasets
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.appChartDatasets &&
      !changes.appChartDatasets.isFirstChange &&
      this._chart !== null &&
      changes.appChartDatasets.currentValue !== null) {
      this._chart.data.datasets = changes.appChartDatasets.currentValue;
      this._chart.update({
        duration: 800,
        easing: 'easeOutBounce'
      });
    }
  }
}
