export interface Plugin {
  id: string;
  name: string;
  type: string;
  enabled: boolean;
  groups: PluginGroup[];
}

export interface PluginGroup {
  name: string;
  key: string;
  multi: boolean;
  type: ObjectType;
  properties: PluginProperty[];
  values: PluginValue[][];
}

export interface PluginProperty {
  name: string;
  key: string;
  desc: string;
  valueType: ValueType;
}

export interface PluginValue {
  key: string;
  value: any;
}

export enum ObjectType {
  None,
  User,
  Settings
}

export enum ValueType {
  None,
  String,
  Boolean
}
