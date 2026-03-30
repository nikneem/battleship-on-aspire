import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'bat-game-route-shell',
  imports: [RouterLink],
  templateUrl: './game-route-shell.html',
  styleUrl: './game-route-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'game-route-shell-host'
  }
})
export class GameRouteShell {
  protected readonly gameCode = inject(ActivatedRoute).snapshot.paramMap.get('gameCode') ?? '';
}
