import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, input, signal } from '@angular/core';

const meterBarCount = 28;
const meterCellCount = 12;

@Component({
  selector: 'bat-landing-vu-meter',
  templateUrl: './landing-vu-meter.html',
  styleUrl: './landing-vu-meter.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingVuMeter {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private animationTimer: number | null = null;

  readonly label = input('');
  readonly status = input('');
  readonly value = input(0);

  protected readonly cellIndexes = Array.from({ length: meterCellCount }, (_, index) => index);
  private readonly barLevels = signal<readonly number[]>(this.generateBarLevels(0));
  protected readonly normalizedValue = computed(() => Math.max(0, Math.min(100, this.value())));
  protected readonly bars = computed(() =>
    this.barLevels().map((activeCells, index) => ({
      index,
      activeCells,
      peakCellIndex: activeCells > 0 ? activeCells - 1 : null
    }))
  );

  constructor() {
    effect(() => {
      this.barLevels.set(this.generateBarLevels(this.normalizedValue()));
    });

    if (this.document.defaultView !== null) {
      this.scheduleAnimation();
    }

    this.destroyRef.onDestroy(() => {
      if (this.animationTimer !== null && this.document.defaultView !== null) {
        this.document.defaultView.clearTimeout(this.animationTimer);
      }
    });
  }

  private scheduleAnimation(): void {
    const view = this.document.defaultView;

    if (view === null) {
      return;
    }

    this.animationTimer = view.setTimeout(() => {
      this.barLevels.set(this.generateBarLevels(this.normalizedValue()));
      this.scheduleAnimation();
    }, this.randomBetween(360, 960));
  }

  private generateBarLevels(value: number): readonly number[] {
    const normalized = value / 100;

    return Array.from({ length: meterBarCount }, (_, index) => {
      const contour = Math.sin((index / (meterBarCount - 1)) * Math.PI) * 1.6;
      const localDrift = this.randomBetween(-2, 3);
      const notch = index % 6 === 0 ? -1 : 0;
      const activeCells = Math.round(normalized * meterCellCount + contour + localDrift + notch);

      return Math.max(0, Math.min(meterCellCount, activeCells));
    });
  }

  private randomBetween(minimum: number, maximumExclusive: number): number {
    return Math.floor(Math.random() * (maximumExclusive - minimum)) + minimum;
  }
}
