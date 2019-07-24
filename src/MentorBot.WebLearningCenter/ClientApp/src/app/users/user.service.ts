import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserInfo } from './user.models';

@Injectable()
export class UserService {
  static readonly url = 'get-users';

  constructor(private http: HttpClient) { }

  public getUsers(): Observable<UserInfo[]> {
    return this.http.get<UserInfo[]>(UserService.url);
  }
}
