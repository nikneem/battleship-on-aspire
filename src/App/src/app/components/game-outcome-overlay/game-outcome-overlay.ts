import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'bat-game-outcome-overlay',
  templateUrl: './game-outcome-overlay.html',
  styleUrl: './game-outcome-overlay.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameOutcomeOverlayComponent {
  readonly outcome = input.required<'winner' | 'loser'>();
  readonly backToMain = output<void>();

  protected onBackToMain(): void {
    this.backToMain.emit();
  }
}
