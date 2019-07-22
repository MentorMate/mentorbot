import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { UserPageComponent } from './user-page.component';
import { UserService } from './user.service';

@NgModule({
  declarations: [
    UserPageComponent
  ],
  imports: [
    HttpModule,
    CommonModule,
    RouterModule.forChild([{ path: '', component: UserPageComponent }])
  ],
  providers: [UserService]
})
export class UsersModule { }
