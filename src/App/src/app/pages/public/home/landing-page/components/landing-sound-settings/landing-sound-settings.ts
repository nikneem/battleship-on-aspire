import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { type BattleOpsSoundSettings } from '../../../../../../battle-ops-sound-settings.service';

@Component({
  selector: 'bat-landing-sound-settings',
  imports: [],
  templateUrl: './landing-sound-settings.html',
  styleUrl: './landing-sound-settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingSoundSettings {
  readonly settings = input.required<BattleOpsSoundSettings>();

  readonly effectsVolumeChange = output<number>();
  readonly musicVolumeChange = output<number>();

  protected readonly overlayOpen = signal(false);

  protected toggleOverlay(): void {
    this.overlayOpen.update((isOpen) => !isOpen);
  }

  protected closeOverlay(): void {
    this.overlayOpen.set(false);
  }

  protected updateEffectsVolume(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.effectsVolumeChange.emit(Number(target.value));
    }
  }

  protected updateMusicVolume(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.musicVolumeChange.emit(Number(target.value));
    }
  }
}
