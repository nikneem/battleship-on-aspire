import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bat-landing-mission',
  imports: [],
  templateUrl: './landing-mission.html',
  styleUrl: './landing-mission.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingMission {
  readonly missionHighlights = input<readonly string[]>([]);
}
