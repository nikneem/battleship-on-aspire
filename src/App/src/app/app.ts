import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { BattleOpsBackgroundMusicService } from './battle-ops-background-music.service';

@Component({
  selector: 'bat-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  private readonly backgroundMusicService = inject(BattleOpsBackgroundMusicService);

  protected readonly soundtrackReady = this.backgroundMusicService;
}
