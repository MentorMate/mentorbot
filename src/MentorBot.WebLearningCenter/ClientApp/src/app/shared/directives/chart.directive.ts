import { AfterContentInit, Directive, ElementRef, Input, NgZone, OnChanges, SimpleChanges } from "@angular/core";
import { Chart, ChartDataSets } from 'chart.js';

@Directive({
  selector: 'canvas[chart][chart-datasets]'
})
export class ChartDirective implements AfterContentInit, OnChanges {
  @Input('chart') chartType: 'pie' | 'line' | 'bar' | 'radar' | 'polarArea' | 'bubble';
  @Input('chart-labels') labels: string[];
  @Input('chart-datasets') datasets: ChartDataSets[];

  private _chart: Chart = null;

  constructor(private readonly _element: ElementRef) {
  }

  ngAfterContentInit(): void {
    if (this._chart !== null) {
      this._chart.destroy();
    }

    this._chart = new Chart(this._element.nativeElement, {
      type: this.chartType,
      data: {
        labels: this.labels || [],
        datasets: !this.datasets || !Array.isArray(this.datasets) ? [] : this.datasets
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.datasets &&
      !changes.datasets.isFirstChange &&
      this._chart !== null &&
      changes.datasets.currentValue !== null) {
      this._chart.data.datasets = changes.datasets.currentValue;
      this._chart.update({
        duration: 800,
        easing: 'easeOutBounce'
      });
    }
  }
}
