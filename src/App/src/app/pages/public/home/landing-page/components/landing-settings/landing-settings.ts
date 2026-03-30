import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { type AccentIntensity, type BattleOpsStyleSettings, type DensityMode } from '../../../../../../battle-ops-style-settings.service';

@Component({
  selector: 'bat-landing-settings',
  imports: [],
  templateUrl: './landing-settings.html',
  styleUrl: './landing-settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingSettings {
  readonly settings = input.required<BattleOpsStyleSettings>();
  readonly densityOptions = input<readonly { label: string; value: DensityMode; hint: string }[]>([]);
  readonly accentOptions = input<readonly { label: string; value: AccentIntensity; hint: string }[]>([]);

  readonly reducedMotionChange = output<Event>();
  readonly densityChange = output<DensityMode>();
  readonly accentIntensityChange = output<AccentIntensity>();

  protected updateReducedMotion(event: Event): void {
    this.reducedMotionChange.emit(event);
  }

  protected updateDensity(value: DensityMode): void {
    this.densityChange.emit(value);
  }

  protected updateAccentIntensity(value: AccentIntensity): void {
    this.accentIntensityChange.emit(value);
  }
}
