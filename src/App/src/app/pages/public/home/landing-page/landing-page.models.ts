export interface GaugeReadout {
  readonly label: string;
  readonly value: number;
  readonly status: string;
}

export interface TelemetryItem {
  readonly label: string;
  readonly value: string;
}
