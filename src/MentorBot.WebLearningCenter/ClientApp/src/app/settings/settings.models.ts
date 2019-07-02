export interface ProcessorSettings {
  name: string;
  enabled: boolean;
  data: Array<{ key: string; value: string; }> | null;
}

export interface Settings {
  key: string;
  processors: Array<ProcessorSettings>;
}
