import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { UserPageComponent } from './components/user-page/user-page.component';
import { UserService } from './user.service';

@NgModule({
  declarations: [
    UserPageComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([{ path: '', component: UserPageComponent }])
  ],
  providers: [UserService]
})
export class UsersModule { }
