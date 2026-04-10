import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, output, signal } from '@angular/core';

@Component({
  selector: 'bat-connection-lost-overlay',
  templateUrl: './connection-lost-overlay.html',
  styleUrl: './connection-lost-overlay.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'connection-lost-overlay-host' }
})
export class ConnectionLostOverlayComponent implements OnInit, OnDestroy {
  readonly secondsRemaining = signal(60);
  readonly timedOut = output<void>();

  private timerId: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.timerId = setInterval(() => {
      const next = this.secondsRemaining() - 1;
      this.secondsRemaining.set(next);
      if (next <= 0) {
        this.stopTimer();
        this.timedOut.emit();
      }
    }, 1000);
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  private stopTimer(): void {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }
}
