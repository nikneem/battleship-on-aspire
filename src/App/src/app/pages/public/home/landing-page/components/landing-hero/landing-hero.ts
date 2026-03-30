import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { type GaugeReadout } from '../../landing-page.models';
import { LandingTerminal } from '../landing-terminal/landing-terminal';
import { LandingVuMeter } from '../landing-vu-meter/landing-vu-meter';

@Component({
  selector: 'bat-landing-hero',
  imports: [LandingTerminal, LandingVuMeter, RouterLink],
  templateUrl: './landing-hero.html',
  styleUrl: './landing-hero.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingHero {
  readonly audioInstruction = input('');
  readonly gaugeReadouts = input<readonly GaugeReadout[]>([]);
  readonly terminalFeed = input<readonly string[]>([]);
}
