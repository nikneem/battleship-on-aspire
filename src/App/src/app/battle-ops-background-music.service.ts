import { DOCUMENT } from '@angular/common';
import { DestroyRef, effect, inject, Injectable } from '@angular/core';

import { BattleOpsSoundSettingsService } from './battle-ops-sound-settings.service';

const soundtrack = ['audio/background-song-01.mp3', 'audio/background-song-02.mp3'] as const;

@Injectable({ providedIn: 'root' })
export class BattleOpsBackgroundMusicService {
  private readonly destroyRef = inject(DestroyRef);
  private readonly document = inject(DOCUMENT);
  private readonly soundSettingsService = inject(BattleOpsSoundSettingsService);
  private readonly audio =
    this.document.defaultView !== null ? new this.document.defaultView.Audio(soundtrack[0]) : null;
  private currentTrackIndex = 0;
  private playbackUnlocked = false;
  private interactionHandler: ((event: Event) => void) | null = null;

  constructor() {
    if (this.audio === null) {
      return;
    }

    this.audio.preload = 'auto';
    this.audio.loop = false;
    this.audio.addEventListener('ended', this.handleTrackEnded);

    effect(() => {
      if (this.audio === null) {
        return;
      }

      this.audio.volume = this.soundSettingsService.musicVolume() / 100;

      if (this.playbackUnlocked && this.soundSettingsService.musicVolume() > 0 && this.audio.paused) {
        void this.tryPlayCurrentTrack();
      }
    });

    this.attachInteractionHandler();

    this.destroyRef.onDestroy(() => {
      if (this.interactionHandler !== null) {
        this.document.removeEventListener('pointerdown', this.interactionHandler);
        this.document.removeEventListener('keydown', this.interactionHandler);
      }

      if (this.audio !== null) {
        this.audio.removeEventListener('ended', this.handleTrackEnded);
        this.audio.pause();
      }
    });
  }

  private attachInteractionHandler(): void {
    if (this.interactionHandler !== null) {
      return;
    }

    this.interactionHandler = () => {
      this.playbackUnlocked = true;

      if (this.interactionHandler !== null) {
        this.document.removeEventListener('pointerdown', this.interactionHandler);
        this.document.removeEventListener('keydown', this.interactionHandler);
        this.interactionHandler = null;
      }

      if (this.soundSettingsService.musicVolume() > 0) {
        void this.tryPlayCurrentTrack();
      }
    };

    this.document.addEventListener('pointerdown', this.interactionHandler, { passive: true });
    this.document.addEventListener('keydown', this.interactionHandler);
  }

  private async tryPlayCurrentTrack(): Promise<void> {
    if (this.audio === null || this.soundSettingsService.musicVolume() <= 0) {
      return;
    }

    try {
      await this.audio.play();
    } catch {
      this.playbackUnlocked = false;
      this.attachInteractionHandler();
    }
  }

  private readonly handleTrackEnded = (): void => {
    if (this.audio === null) {
      return;
    }

    this.currentTrackIndex = (this.currentTrackIndex + 1) % soundtrack.length;
    this.audio.src = soundtrack[this.currentTrackIndex];
    this.audio.load();

    if (this.playbackUnlocked && this.soundSettingsService.musicVolume() > 0) {
      void this.tryPlayCurrentTrack();
    }
  };
}
