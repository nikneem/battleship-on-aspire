import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { type GaugeReadout, type RadarContact, type TelemetryItem } from '../../landing-page.models';
import { LandingRadar } from '../landing-radar/landing-radar';
import { LandingTerminal } from '../landing-terminal/landing-terminal';

@Component({
  selector: 'bat-landing-hero',
  imports: [LandingRadar, LandingTerminal],
  templateUrl: './landing-hero.html',
  styleUrl: './landing-hero.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingHero {
  readonly telemetry = input<readonly TelemetryItem[]>([]);
  readonly audioInstruction = input('');
  readonly gaugeReadouts = input<readonly GaugeReadout[]>([]);
  readonly radarContacts = input<readonly RadarContact[]>([]);
  readonly terminalFeed = input<readonly string[]>([]);
}
