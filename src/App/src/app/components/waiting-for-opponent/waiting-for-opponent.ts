import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';

import { MiniPongComponent } from '../mini-pong/mini-pong';

@Component({
  selector: 'bat-waiting-for-opponent',
  imports: [MiniPongComponent],
  templateUrl: './waiting-for-opponent.html',
  styleUrl: './waiting-for-opponent.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WaitingForOpponentDialogComponent {
  readonly gameCode = input.required<string>();

  protected readonly copied = signal(false);

  protected copyCode(): void {
    void navigator.clipboard.writeText(this.gameCode()).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    });
  }
}
