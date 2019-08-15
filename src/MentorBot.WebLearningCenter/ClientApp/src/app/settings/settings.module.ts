import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { SettingsPageComponent } from './components/settings-page/settings-page.component';
import { SettingsService } from './settings.service';

@NgModule({
  declarations: [
    SettingsPageComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: SettingsPageComponent }])
  ],
  providers: [SettingsService]
})
export class SettingsModule { }
