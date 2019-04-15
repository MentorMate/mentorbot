export interface ProcessorSettings {
  name: string;
  enabled: boolean;
}

export interface Settings {
  key: string;
  processors: Array<ProcessorSettings>;
}
