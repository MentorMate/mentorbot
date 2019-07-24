import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { UserService } from './user.service';
import { UserInfo } from './user.models';

const template = `
  <h2>Users</h2>
  <table class="users">
    <thead>
      <tr>
        <th>Name</th>
        <th class="email">Email</th>
        <th>Role</th>
        <th>Department</th>
        <th>Manager</th>
        <th class="cust">Customers</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let user of users$ | async">
        <td>{{user.name}}</td>
        <td>{{user.email}}</td>
        <td>{{user.role}}</td>
        <td>{{user.department}}</td>
        <td>{{user.manager}}</td>
        <td>{{user.customers}}</td>
      </tr>
    </tbody>
  </table>
`;

const style = `
  :host {
    display: block;
  }

  .email, .cust {
    width: 400px;
    max-width: 450px;
  }
`;

@Component({
  selector: 'lp-users',
  template: template,
  styles: [style]
})
export class UserPageComponent implements OnInit {
  users$: Observable<UserInfo[]>
  constructor(
    private readonly service: UserService) { }

  ngOnInit(): void {
    this.users$ = this.service.getUsers();
  }
}
