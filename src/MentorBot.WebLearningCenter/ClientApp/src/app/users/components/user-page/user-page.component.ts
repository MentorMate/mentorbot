import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { UserService } from '../../user.service';
import { UserInfo } from '../../user.models';
import { PluginGroup } from '../../../settings/settings.models';
import { SettingsService } from '../../../settings/settings.service';
import { take, map, tap } from 'rxjs/operators';

const style = `
  :host {
    display: block;
  }

  .email, .cust {
    width: 400px;
    max-width: 450px;
  }

  .left-align {
    text-align: left;
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
  }

  .title {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
    font-weight: 100;
  }

.table-edit-row {
  border-top: 1px solid #CCC;
  border-bottom: 1px solid #CCC;
  padding: 0 10px 10px 10px;
}

.values { max-width: 600px; display: block; }
`;

@Component({
  selector: 'app-users',
  templateUrl: './user-page.component.html',
  styles: [style],
})
export class UserPageComponent implements OnInit {
  editingId: string | null = null;
  users$?: Observable<UserInfo[]>;
  properties$?: Observable<PluginGroup[]>;

  constructor(private readonly _userService: UserService, private readonly _settingsService: SettingsService) {}

  ngOnInit(): void {
    this.users$ = this._userService.getUsers().pipe(
      tap(users =>
        users.forEach(user => {
          if (user.properties === null || typeof user.properties === 'undefined') {
            user.properties = {};
          }
        })
      )
    );

    this.properties$ = this._settingsService.getPlugins().pipe(
      take(1),
      map(plugins =>
        plugins
          .filter(it => it.enabled && it.groups !== null && it.groups.length > 0)
          .map(it => it.groups)
          .reduce((a, b) => a.concat(b))
          .filter(it => it.type === 1)
      )
    );
  }

  edit(user: UserInfo): void {
    this.editingId = this.editingId === user.id ? null : user.id;
  }

  save(user: UserInfo): void {
    this._userService
      .saveUserProperties(user)
      .pipe(take(1))
      .subscribe(() => (this.editingId = null));
  }
}
