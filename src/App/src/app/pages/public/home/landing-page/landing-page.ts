import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { BattleOpsStyleSettingsService, type AccentIntensity, type DensityMode } from '../../../../battle-ops-style-settings.service';
import { LandingDoctrine } from './components/landing-doctrine/landing-doctrine';
import { LandingHero } from './components/landing-hero/landing-hero';
import { LandingMission } from './components/landing-mission/landing-mission';
import { LandingSettings } from './components/landing-settings/landing-settings';
import { type GaugeReadout, type RadarContact } from './landing-page.models';
import { terminalMessages } from './terminal-messages';

@Component({
  selector: 'bat-landing-page',
  imports: [LandingHero, LandingSettings, LandingDoctrine, LandingMission],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'landing-page-host'
  }
})
export class LandingPage {
  private readonly styleSettingsService = inject(BattleOpsStyleSettingsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly document = inject(DOCUMENT);
  private readonly audio =
    this.document.defaultView !== null ? new this.document.defaultView.Audio('audio/sonar-pulse.mp3') : null;
  private terminalTimer: number | null = null;
  private gaugeTimer: number | null = null;
  private sonarTimer: number | null = null;
  private interactionHandler: ((event: Event) => void) | null = null;
  private messageCursor = 0;

  protected readonly settings = this.styleSettingsService.settings.asReadonly();
  protected readonly densityOptions = [
    { label: 'COMPACT', value: 'compact', hint: 'tight tactical spacing' },
    { label: 'STANDARD', value: 'standard', hint: 'balanced command spacing' },
    { label: 'RELAXED', value: 'relaxed', hint: 'expanded surface rhythm' }
  ] as const;
  protected readonly accentOptions = [
    { label: 'STANDARD', value: 'standard', hint: 'precise cyan glow' },
    { label: 'SURGE', value: 'surge', hint: 'elevated sonar energy' }
  ] as const;
  protected readonly defenseStrategies = [
    'Split the fleet to draw hostile pings away from the primary command lane.',
    'Favor passive sonar first, then fire only when the contact crosses the reticle.',
    'Use AI-assisted prompts to refine tactics before each engagement sequence.'
  ] as const;
  protected readonly missionHighlights = [
    'Teach AI-driven development through real delivery loops.',
    'Show how Aspire coordinates supporting services across the solution.',
    'Demonstrate disciplined frontend execution with modern Angular patterns.'
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
    { label: 'MOTION', value: this.settings().reducedMotion ? 'REDUCED' : 'FULL' },
    { label: 'DENSITY', value: this.settings().density.toUpperCase() },
    { label: 'ACCENT', value: this.settings().accentIntensity.toUpperCase() },
    { label: 'SONAR', value: this.sonarStatus() }
  ]);
  protected readonly audioInstruction = computed(() =>
    this.sonarStatus() === 'BLOCKED'
      ? 'Tap anywhere in the control station to arm sonar playback.'
      : 'Ambient sonar pulses cycle every 10 to 15 seconds when permitted by the browser.'
  );

  constructor() {
    this.seedTerminalFeed();

    if (this.audio !== null) {
      this.audio.preload = 'auto';
      this.audio.volume = 0.2;
      this.audio.addEventListener('ended', this.handleSonarEnd);
    }

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

  protected updateReducedMotion(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.styleSettingsService.setReducedMotion(target.checked);
    }
  }

  protected setDensity(value: DensityMode): void {
    this.styleSettingsService.setDensity(value);
  }

  protected setAccentIntensity(value: AccentIntensity): void {
    this.styleSettingsService.setAccentIntensity(value);
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
