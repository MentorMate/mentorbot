import { PluginValue } from '../settings/settings.models';

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  role: string;
  manager: string;
  department: string;
  customers: string;
  properties: Dictionary<PluginValue[][]>;
}
