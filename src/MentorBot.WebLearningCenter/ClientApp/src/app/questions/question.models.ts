import { PluginValue } from '../settings/settings.models';

export interface Question {
  name: string;
  subQuestions: Question[];
}
