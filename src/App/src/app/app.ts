import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { BattleOpsBackgroundMusicService } from './battle-ops-background-music.service';
import { BattleOpsSoundSettingsService } from './battle-ops-sound-settings.service';
import { LandingSoundSettings } from './pages/public/home/landing-page/components/landing-sound-settings/landing-sound-settings';

@Component({
  selector: 'bat-root',
  imports: [RouterOutlet, LandingSoundSettings],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  private readonly backgroundMusicService = inject(BattleOpsBackgroundMusicService);
  private readonly soundSettingsService = inject(BattleOpsSoundSettingsService);

  protected readonly soundtrackReady = this.backgroundMusicService;
  protected readonly soundSettings = this.soundSettingsService.settings.asReadonly();

  protected setEffectsVolume(value: number): void {
    this.soundSettingsService.setEffectsVolume(value);
  }

  protected setMusicVolume(value: number): void {
    this.soundSettingsService.setMusicVolume(value);
  }
}
