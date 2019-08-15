import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DashboardPageComponent } from './components/dashboard-page/dashboard-page.component';
import { DashboardService } from './dashboard.service';

@NgModule({
  declarations: [
    DashboardPageComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([{ path: '', component: DashboardPageComponent }])
  ],
  providers: [DashboardService]
})
export class DashboardModule { }
