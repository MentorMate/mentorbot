import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { PluginValuesComponent } from './components/plugin-values/plugin-values.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule
  ],
  declarations: [
    PluginValuesComponent
  ],
  exports: [
    PluginValuesComponent
  ]
})
export class SharedModule { }
