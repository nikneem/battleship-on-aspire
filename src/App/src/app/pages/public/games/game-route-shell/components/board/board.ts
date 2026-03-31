import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import type { ShipPlacement, ShotRecord } from '../../../../../../games-api.service';

export interface SelectedCell {
  readonly row: number;
  readonly column: number;
}

interface BoardCellVm {
  readonly key: string;
  readonly row: number;
  readonly column: number;
  readonly label: string;
  readonly isShip: boolean;
  readonly isSelected: boolean;
  readonly shot: ShotRecord | null;
}

const boardDimension = 10;
const rowLabels = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'] as const;

function getShipCells(ship: ShipPlacement): Array<{ row: number; col: number }> {
  return Array.from({ length: ship.length }, (_, i) => ({
    row: ship.start.row + (ship.orientation === 1 ? i : 0),
    col: ship.start.column + (ship.orientation === 0 ? i : 0)
  }));
}

@Component({
  selector: 'bat-board',
  imports: [],
  templateUrl: './board.html',
  styleUrl: './board.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoardComponent {
  readonly mode = input.required<'attack' | 'defend'>();
  readonly ships = input<readonly ShipPlacement[]>([]);
  readonly shots = input<readonly ShotRecord[]>([]);
  readonly selectedCell = input<SelectedCell | null>(null);

  readonly cellSelected = output<SelectedCell>();

  protected readonly boardColumns = Array.from({ length: boardDimension }, (_, i) => i + 1);
  protected readonly rowLabels = rowLabels;

  protected readonly cells = computed<readonly BoardCellVm[]>(() => {
    const mode = this.mode();
    const ships = this.ships();
    const shots = this.shots();
    const selected = this.selectedCell();

    const shotMap = new Map(shots.map((s) => [`${s.coordinate.row}-${s.coordinate.column}`, s]));

    const shipCellSet = new Set<string>();
    if (mode === 'defend') {
      for (const ship of ships) {
        for (const cell of getShipCells(ship)) {
          shipCellSet.add(`${cell.row}-${cell.col}`);
        }
      }
    }

    return Array.from({ length: boardDimension * boardDimension }, (_, index) => {
      const row = Math.floor(index / boardDimension);
      const column = index % boardDimension;
      const key = `${row}-${column}`;

      return {
        key,
        row,
        column,
        label: `${rowLabels[row]}${column + 1}`,
        isShip: shipCellSet.has(key),
        isSelected: selected !== null && selected.row === row && selected.column === column,
        shot: shotMap.get(key) ?? null
      };
    });
  });

  protected clickCell(row: number, column: number): void {
    if (this.mode() !== 'attack') return;
    const alreadyFired = this.shots().some(
      (s) => s.coordinate.row === row && s.coordinate.column === column
    );
    if (alreadyFired) return;
    this.cellSelected.emit({ row, column });
  }
}
