import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';

import { SettingsPageComponent } from './settings-page.component';
import { SettingsService } from './settings.service';

@NgModule({
  declarations: [
    SettingsPageComponent
  ],
  imports: [
    HttpModule,
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: SettingsPageComponent }])
  ],
  providers: [SettingsService]
})
export class SettingsModule { }
