import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';

import { BattleOpsSoundSettingsService } from './battle-ops-sound-settings.service';

const missileLaunchSounds = [
  'audio/missle-launch-01.mp3',
  'audio/missle-launch-02.mp3',
  'audio/missle-launch-03.mp3',
  'audio/missle-launch-04.mp3'
] as const;

const hitExplosionSounds = [
  'audio/hit-explosion-01.mp3',
  'audio/hit-explosion-02.mp3',
  'audio/hit-explosion-03.mp3'
] as const;

const missSplashSounds = ['audio/miss-splash-01.mp3', 'audio/miss-splash-02.mp3'] as const;

@Injectable({ providedIn: 'root' })
export class BattleOpsSoundEffectsService {
  private readonly document = inject(DOCUMENT);
  private readonly soundSettings = inject(BattleOpsSoundSettingsService);

  playMissileLaunch(): void {
    this.playRandom([...missileLaunchSounds]);
  }

  playHitExplosion(): void {
    this.playRandom([...hitExplosionSounds]);
  }

  playMissSplash(): void {
    this.playRandom([...missSplashSounds]);
  }

  private playRandom(sounds: string[]): void {
    const volume = this.soundSettings.effectsVolume() / 100;
    if (volume <= 0 || this.document.defaultView === null) {
      return;
    }

    const src = sounds[Math.floor(Math.random() * sounds.length)];
    const audio = new this.document.defaultView.Audio(src);
    audio.volume = volume;
    void audio.play().catch(() => {
      // Playback may be blocked before user interaction — ignore silently
    });
  }
}
