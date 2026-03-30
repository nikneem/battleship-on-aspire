import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { BattleOpsSoundSettingsService } from '../../../../battle-ops-sound-settings.service';
import { LandingDoctrine } from './components/landing-doctrine/landing-doctrine';
import { LandingHero } from './components/landing-hero/landing-hero';
import { LandingMission } from './components/landing-mission/landing-mission';
import { LandingRadar } from './components/landing-radar/landing-radar';
import { LandingSoundSettings } from './components/landing-sound-settings/landing-sound-settings';
import { type GaugeReadout, type RadarContact } from './landing-page.models';
import { terminalMessages } from './terminal-messages';

@Component({
  selector: 'bat-landing-page',
  imports: [LandingHero, LandingDoctrine, LandingMission, LandingSoundSettings, LandingRadar],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'landing-page-host'
  }
})
export class LandingPage {
  private readonly soundSettingsService = inject(BattleOpsSoundSettingsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly document = inject(DOCUMENT);
  private readonly audio =
    this.document.defaultView !== null ? new this.document.defaultView.Audio('audio/sonar-pulse.mp3') : null;
  private terminalTimer: number | null = null;
  private gaugeTimer: number | null = null;
  private sonarTimer: number | null = null;
  private interactionHandler: ((event: Event) => void) | null = null;
  private messageCursor = 0;

  protected readonly soundSettings = this.soundSettingsService.settings.asReadonly();
  protected readonly defenseStrategies = [
    'Queue up a live Battleship board and track every hit, miss, and sunk ship from one command surface.',
    'Use the sonar feed to stay locked on the latest moves before you commit your next shot.',
    'Open the game, place your fleet, and start trading volleys as soon as the board is ready.'
  ] as const;
  protected readonly missionHighlights = [
    'Launch a playable Battleship match directly from the Battle Ops experience.',
    'Deploy your fleet, call shots, and watch the tactical feed react in real time.',
    'Use the landing page as the briefing room before you enter the actual game board.'
  ] as const;
  protected readonly terminalFeed = signal<string[]>([]);
  protected readonly gaugeReadouts = signal<GaugeReadout[]>([
    { label: 'HULL INTEGRITY', value: 92, status: 'STABLE' },
    { label: 'SONAR GAIN', value: 74, status: 'TRACKING' },
    { label: 'REACTOR LOAD', value: 61, status: 'BALANCED' }
  ]);
  protected readonly radarContacts = signal<RadarContact[]>([
    { id: 'alpha', top: '26%', left: '62%', coordinates: 'D4', code: 'SABLE-17', pulseDelay: '0s' },
    { id: 'bravo', top: '58%', left: '30%', coordinates: 'H7', code: 'EMBER-42', pulseDelay: '0.9s' },
    { id: 'charlie', top: '70%', left: '57%', coordinates: 'C8', code: 'NOVA-88', pulseDelay: '1.6s' }
  ]);
  protected readonly sonarStatus = signal<'STANDBY' | 'BLOCKED' | 'ACTIVE'>('STANDBY');
  protected readonly telemetry = computed(() => [
    { label: 'GAME MODE', value: 'LIVE MATCH' },
    { label: 'BOARD SIZE', value: '10 X 10' },
    { label: 'PLAYER STATUS', value: 'READY' },
    { label: 'SONAR', value: this.sonarStatus() }
  ]);
  protected readonly audioInstruction = computed(() =>
    this.sonarStatus() === 'BLOCKED'
      ? 'Tap anywhere in the control station to arm sonar playback.'
      : 'Arm your fleet, open the sonar feed, and then jump into a Battleship match.'
  );

  constructor() {
    this.seedTerminalFeed();

    if (this.audio !== null) {
      this.audio.preload = 'auto';
      this.audio.addEventListener('ended', this.handleSonarEnd);
    }

    effect(() => {
      if (this.audio !== null) {
        this.audio.volume = this.soundSettings().effectsVolume / 100;
      }
    });

    if (this.document.defaultView !== null) {
      this.scheduleTerminalUpdate();
      this.scheduleGaugeRefresh();
      this.scheduleSonarPulse();
      this.attachInteractionHandler();
    }

    this.destroyRef.onDestroy(() => {
      const view = this.document.defaultView;

      if (view !== null) {
        if (this.terminalTimer !== null) {
          view.clearTimeout(this.terminalTimer);
        }

        if (this.gaugeTimer !== null) {
          view.clearTimeout(this.gaugeTimer);
        }

        if (this.sonarTimer !== null) {
          view.clearTimeout(this.sonarTimer);
        }
      }

      if (this.interactionHandler !== null) {
        this.document.removeEventListener('pointerdown', this.interactionHandler);
        this.document.removeEventListener('keydown', this.interactionHandler);
      }

      if (this.audio !== null) {
        this.audio.removeEventListener('ended', this.handleSonarEnd);
      }
    });
  }

  protected setEffectsVolume(value: number): void {
    this.soundSettingsService.setEffectsVolume(value);
  }

  protected setMusicVolume(value: number): void {
    this.soundSettingsService.setMusicVolume(value);
  }

  private seedTerminalFeed(): void {
    this.terminalFeed.set([
      this.formatTerminalMessage(terminalMessages[0]),
      this.formatTerminalMessage(terminalMessages[1]),
      this.formatTerminalMessage(terminalMessages[2]),
      this.formatTerminalMessage(terminalMessages[3])
    ]);
    this.messageCursor = 4;
  }

  private scheduleTerminalUpdate(): void {
    const view = this.document.defaultView;

    if (view === null) {
      return;
    }

    this.terminalTimer = view.setTimeout(() => {
      this.pushTerminalMessage();
      this.scheduleTerminalUpdate();
    }, this.randomBetween(3200, 5200));
  }

  private scheduleGaugeRefresh(): void {
    const view = this.document.defaultView;

    if (view === null) {
      return;
    }

    this.gaugeTimer = view.setTimeout(() => {
      this.gaugeReadouts.update((gauges) =>
        gauges.map((gauge) => {
          const drift = this.randomBetween(-8, 9);
          const nextValue = Math.max(35, Math.min(99, gauge.value + drift));

          return {
            label: gauge.label,
            value: nextValue,
            status: nextValue >= 80 ? 'OPTIMAL' : nextValue >= 60 ? 'TRACKING' : 'WATCH'
          };
        })
      );
      this.scheduleGaugeRefresh();
    }, this.randomBetween(2800, 4600));
  }

  private scheduleSonarPulse(): void {
    const view = this.document.defaultView;

    if (view === null) {
      return;
    }

    this.sonarTimer = view.setTimeout(() => {
      void this.playSonarPulse();
      this.scheduleSonarPulse();
    }, this.randomBetween(10000, 15001));
  }

  private pushTerminalMessage(): void {
    const nextMessage = terminalMessages[this.messageCursor % terminalMessages.length];
    this.messageCursor += 1;

    this.terminalFeed.update((feed) => [...feed.slice(-5), this.formatTerminalMessage(nextMessage)]);
  }

  private formatTerminalMessage(message: string): string {
    const minute = 10 + (this.messageCursor % 40);
    const second = (this.messageCursor * 7) % 60;

    return `[09:${minute.toString().padStart(2, '0')}:${second.toString().padStart(2, '0')}] ${message}`;
  }

  private attachInteractionHandler(): void {
    if (this.interactionHandler !== null) {
      return;
    }

    this.interactionHandler = () => {
      if (this.sonarStatus() === 'BLOCKED') {
        void this.playSonarPulse();
      }
    };

    this.document.addEventListener('pointerdown', this.interactionHandler, { passive: true });
    this.document.addEventListener('keydown', this.interactionHandler);
  }

  private async playSonarPulse(): Promise<void> {
    if (this.audio === null) {
      return;
    }

    try {
      this.audio.currentTime = 0;
      await this.audio.play();
      this.sonarStatus.set('ACTIVE');
    } catch {
      this.sonarStatus.set('BLOCKED');
    }
  }

  private readonly handleSonarEnd = (): void => {
    this.sonarStatus.set('STANDBY');
  };

  private randomBetween(minimum: number, maximumExclusive: number): number {
    return Math.floor(Math.random() * (maximumExclusive - minimum)) + minimum;
  }
}
