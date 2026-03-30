import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { type GaugeReadout, type TelemetryItem } from '../../landing-page.models';
import { LandingTerminal } from '../landing-terminal/landing-terminal';

@Component({
  selector: 'bat-landing-hero',
  imports: [LandingTerminal, RouterLink],
  templateUrl: './landing-hero.html',
  styleUrl: './landing-hero.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingHero {
  readonly telemetry = input<readonly TelemetryItem[]>([]);
  readonly audioInstruction = input('');
  readonly gaugeReadouts = input<readonly GaugeReadout[]>([]);
  readonly terminalFeed = input<readonly string[]>([]);
}
