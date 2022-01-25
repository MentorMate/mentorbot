import '../../../shared/extensions';

import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';

import { SettingsService } from '../../settings.service';
import { Plugin, ObjectType, PluginGroup } from '../../settings.models';

@Component({
  selector: 'app-settings',
  templateUrl: './settings-page.component.html',
  styleUrls: ['./settings-page.component.scss'],
})
export class SettingsPageComponent implements OnInit {
  plugins: Plugin[] | null = null;
  pluginsGroups: { plugin: Plugin; groups: PluginGroup[] }[] | null = null;

  constructor(private service: SettingsService) {}

  ngOnInit(): void {
    this.service
      .getPlugins()
      .pipe(take(1))
      .subscribe(it => this.load(it));
  }

  save(): void {
    if (this.plugins !== null) {
      this.service
        .savePlugins(this.plugins)
        .pipe(take(1))
        .subscribe(() => this.ngOnInit());
    }
  }

  load(plugins: Plugin[]): void {
    this.plugins = plugins;
    this.pluginsGroups = plugins
      .filter(plugin => plugin.groups !== null && plugin.groups.length > 0)
      .map(plugin => ({ plugin, groups: plugin.groups.filter(group => group.type === ObjectType.Settings) }));
  }
}
