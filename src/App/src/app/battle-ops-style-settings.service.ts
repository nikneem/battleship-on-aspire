import { DOCUMENT } from '@angular/common';
import { effect, inject, Injectable, signal } from '@angular/core';

export type DensityMode = 'compact' | 'standard' | 'relaxed';
export type AccentIntensity = 'standard' | 'surge';

export interface BattleOpsStyleSettings {
  readonly reducedMotion: boolean;
  readonly density: DensityMode;
  readonly accentIntensity: AccentIntensity;
}

const densityModes: readonly DensityMode[] = ['compact', 'standard', 'relaxed'];
const accentIntensityModes: readonly AccentIntensity[] = ['standard', 'surge'];

@Injectable({ providedIn: 'root' })
export class BattleOpsStyleSettingsService {
  private readonly document = inject(DOCUMENT);
  private readonly storage = this.document.defaultView?.localStorage ?? null;

  readonly reducedMotion = signal(this.readReducedMotion());
  readonly density = signal<DensityMode>(this.readDensity());
  readonly accentIntensity = signal<AccentIntensity>(this.readAccentIntensity());
  readonly settings = signal<BattleOpsStyleSettings>({
    reducedMotion: this.reducedMotion(),
    density: this.density(),
    accentIntensity: this.accentIntensity()
  });

  constructor() {
    effect(() => {
      const nextSettings: BattleOpsStyleSettings = {
        reducedMotion: this.reducedMotion(),
        density: this.density(),
        accentIntensity: this.accentIntensity()
      };

      this.settings.set(nextSettings);

      const root = this.document.documentElement;
      root.classList.add('battle-ops-theme');
      root.dataset['motion'] = nextSettings.reducedMotion ? 'reduced' : 'standard';
      root.dataset['density'] = nextSettings.density;
      root.dataset['accent'] = nextSettings.accentIntensity;

      if (this.storage !== null) {
        this.storage.setItem('battle-ops-reduced-motion', nextSettings.reducedMotion ? 'true' : 'false');
        this.storage.setItem('battle-ops-density', nextSettings.density);
        this.storage.setItem('battle-ops-accent-intensity', nextSettings.accentIntensity);
      }
    });
  }

  setReducedMotion(enabled: boolean): void {
    this.reducedMotion.set(enabled);
  }

  setDensity(value: DensityMode): void {
    this.density.set(value);
  }

  setAccentIntensity(value: AccentIntensity): void {
    this.accentIntensity.set(value);
  }

  private readReducedMotion(): boolean {
    return this.storage?.getItem('battle-ops-reduced-motion') === 'true';
  }

  private readDensity(): DensityMode {
    const storedValue = this.storage?.getItem('battle-ops-density');
    return densityModes.includes(storedValue as DensityMode) ? (storedValue as DensityMode) : 'standard';
  }

  private readAccentIntensity(): AccentIntensity {
    const storedValue = this.storage?.getItem('battle-ops-accent-intensity');
    return accentIntensityModes.includes(storedValue as AccentIntensity)
      ? (storedValue as AccentIntensity)
      : 'standard';
  }
}
