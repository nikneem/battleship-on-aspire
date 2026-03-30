import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import {
  AccentIntensity,
  BattleOpsStyleSettingsService,
  DensityMode
} from './battle-ops-style-settings.service';

@Component({
  selector: 'bat-root',
  imports: [NgOptimizedImage, ButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  private readonly styleSettingsService = inject(BattleOpsStyleSettingsService);

  protected readonly settings = this.styleSettingsService.settings.asReadonly();
  protected readonly telemetry = computed(() => [
    { label: 'MOTION', value: this.settings().reducedMotion ? 'REDUCED' : 'FULL' },
    { label: 'DENSITY', value: this.settings().density.toUpperCase() },
    { label: 'ACCENT', value: this.settings().accentIntensity.toUpperCase() }
  ]);
  protected readonly densityOptions = [
    { label: 'COMPACT', value: 'compact', hint: 'tight tactical spacing' },
    { label: 'STANDARD', value: 'standard', hint: 'balanced command spacing' },
    { label: 'RELAXED', value: 'relaxed', hint: 'expanded surface rhythm' }
  ] as const;
  protected readonly accentOptions = [
    { label: 'STANDARD', value: 'standard', hint: 'precise cyan glow' },
    { label: 'SURGE', value: 'surge', hint: 'elevated sonar energy' }
  ] as const;
  protected readonly styleRules = [
    'USE DARK TACTICAL SURFACES BY DEFAULT',
    'KEEP LOGOS ON CLEAN, HIGH-CONTRAST BACKDROPS',
    'DO NOT STRETCH, SHADOW, OR RECOLOR BRAND MARKS',
    'PREFER COMMAND-LIKE LABELS AND COLD STATUS LANGUAGE'
  ] as const;
  protected readonly primengMappings = [
    { component: 'BUTTONS', rule: 'CYAN OUTLINE, GLOW HOVER, COMMAND-LIKE LABELS' },
    { component: 'PANELS', rule: 'MIDNIGHT SURFACES, STEEL SEPARATORS, UPPERCASE HEADERS' },
    { component: 'DIALOGS', rule: 'SQUARED FRAMES, DARK BACKDROP, SONAR ACCENT EDGE' },
    { component: 'TABLES', rule: 'STEEL GRID LINES, AQUA HOVER, CYAN ACTIVE ROW' },
    { component: 'INPUTS', rule: 'TRANSPARENT FIELDS, CYAN EMPHASIS, TACTICAL FOCUS RING' }
  ] as const;
  protected readonly microcopyPreview = [
    { label: 'ALERT', text: 'CONTACT DETECTED' },
    { label: 'ACTION', text: 'DEPLOY SHIPS' },
    { label: 'TOOLTIP', text: 'AWAITING COORDINATES' },
    { label: 'WARNING', text: 'UNAUTHORIZED MOVE' }
  ] as const;

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
}
