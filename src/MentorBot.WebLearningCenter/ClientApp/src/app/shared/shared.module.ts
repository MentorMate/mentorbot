import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

import { PluginValuesComponent } from './components/plugin-values/plugin-values.component';
import { ChartDirective } from './directives/chart.directive';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';

@NgModule({
  imports: [CommonModule, FormsModule, MatCardModule, MatButtonModule],
  declarations: [PluginValuesComponent, ChartDirective, SafeHtmlPipe],
  exports: [PluginValuesComponent, ChartDirective, SafeHtmlPipe],
})
export class SharedModule {}
