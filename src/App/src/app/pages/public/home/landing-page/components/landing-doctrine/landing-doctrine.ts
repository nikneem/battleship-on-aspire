import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bat-landing-doctrine',
  imports: [],
  templateUrl: './landing-doctrine.html',
  styleUrl: './landing-doctrine.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingDoctrine {
  readonly strategies = input<readonly string[]>([]);
}
