import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { UserInfo } from './user.models';

@Injectable()
export class UserService {
  static readonly url = 'get-users';
  static readonly saveUserProps = 'save-user-props';

  constructor(private http: HttpClient) { }

  getUsers(): Observable<UserInfo[]> {
    return this.http.get<UserInfo[]>(UserService.url);
  }

  saveUserProperties(user: UserInfo): Observable<Object> {
    return this.http.post(UserService.saveUserProps, user);
  }
}
