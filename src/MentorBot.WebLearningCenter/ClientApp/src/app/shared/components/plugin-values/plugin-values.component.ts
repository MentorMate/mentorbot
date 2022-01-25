import '../../extensions';

import { Component, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { PluginProperty, PluginValue, PluginGroup } from '../../../settings/settings.models';

@Component({
  selector: 'app-plugin-values',
  templateUrl: './plugin-values.component.html',
  styles: [],
})
export class PluginValuesComponent implements OnChanges {
  pluginsProperties: PropertyValue[][] = [];

  @Input() group: PluginGroup | null = null;
  @Input() values: PluginValue[][] | null = null;
  @Output() valuesChange = new EventEmitter<PluginValue[][]>();

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes['group'] || changes['values']) &&
      typeof this.group !== 'undefined' &&
      this.group !== null &&
      typeof this.values !== 'undefined'
    ) {
      const properties = this.group.properties.toDictionary<PluginProperty>((obj, prop) => (obj[prop.key] = prop));
      this.pluginsProperties = this.values === null ? [] : this.values.map(it => it.map(value => ({ value, prop: properties[value.key] })));
    }
  }

  add(group: PluginGroup): void {
    this.pluginsProperties.push(group.properties.map(prop => ({ prop, value: { key: prop.key, value: '' } })));
    this.onChange();
  }

  remove(values: PropertyValue[]): void {
    const idx = this.pluginsProperties.indexOf(values);
    this.pluginsProperties.splice(idx, 1);
    this.onChange();
  }

  onChange(): void {
    if (this.valuesChange) {
      const values = this.pluginsProperties.map(it => it.map(value => value.value));
      this.valuesChange.emit(values);
    }
  }
}

interface PropertyValue {
  prop: PluginProperty;
  value: PluginValue;
}
