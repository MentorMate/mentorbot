import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';

import { SettingsPageComponent } from './components/settings-page/settings-page.component';
import { SettingsService } from './settings.service';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [SettingsPageComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    RouterModule.forChild([{ path: '', component: SettingsPageComponent }]),
    MatButtonModule,
  ],
  providers: [SettingsService],
})
export class SettingsModule {}
