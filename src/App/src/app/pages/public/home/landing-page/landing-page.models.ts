export interface GaugeReadout {
  readonly label: string;
  readonly value: number;
  readonly status: string;
}

export interface TelemetryItem {
  readonly label: string;
  readonly value: string;
}

export interface RadarContact {
  readonly id: string;
  readonly top: string;
  readonly left: string;
  readonly coordinates: string;
  readonly code: string;
  readonly pulseDelay: string;
}

export interface RadarContact {
  readonly id: string;
  readonly top: string;
  readonly left: string;
  readonly coordinates: string;
  readonly code: string;
  readonly pulseDelay: string;
}
