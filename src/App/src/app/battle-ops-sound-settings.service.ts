import { DOCUMENT } from '@angular/common';
import { effect, inject, Injectable, signal } from '@angular/core';

export interface BattleOpsSoundSettings {
  readonly effectsVolume: number;
  readonly musicVolume: number;
}

const defaultEffectsVolume = 50;
const defaultMusicVolume = 30;

@Injectable({ providedIn: 'root' })
export class BattleOpsSoundSettingsService {
  private readonly document = inject(DOCUMENT);
  private readonly storage = this.document.defaultView?.localStorage ?? null;

  readonly effectsVolume = signal(this.readVolume('battle-ops-effects-volume', defaultEffectsVolume));
  readonly musicVolume = signal(this.readVolume('battle-ops-music-volume', defaultMusicVolume));
  readonly settings = signal<BattleOpsSoundSettings>({
    effectsVolume: this.effectsVolume(),
    musicVolume: this.musicVolume()
  });

  constructor() {
    effect(() => {
      const nextSettings: BattleOpsSoundSettings = {
        effectsVolume: this.effectsVolume(),
        musicVolume: this.musicVolume()
      };

      this.settings.set(nextSettings);

      if (this.storage !== null) {
        this.storage.setItem('battle-ops-effects-volume', nextSettings.effectsVolume.toString());
        this.storage.setItem('battle-ops-music-volume', nextSettings.musicVolume.toString());
      }
    });
  }

  setEffectsVolume(value: number): void {
    this.effectsVolume.set(this.clampVolume(value));
  }

  setMusicVolume(value: number): void {
    this.musicVolume.set(this.clampVolume(value));
  }

  private readVolume(storageKey: string, fallback: number): number {
    const storedValue = this.storage?.getItem(storageKey);

    if (storedValue === null) {
      return fallback;
    }

    const parsedValue = Number(storedValue);
    return Number.isFinite(parsedValue) ? this.clampVolume(parsedValue) : fallback;
  }

  private clampVolume(value: number): number {
    return Math.max(0, Math.min(100, Math.round(value)));
  }
}
