import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AnonymousPlayerIdentityService } from '../../../../anonymous-player-identity.service';

type ShipOrientation = 'horizontal' | 'vertical';

interface ShipDefinition {
  readonly id: string;
  readonly label: string;
  readonly length: number;
  readonly accent: string;
}

interface ShipPlacement {
  readonly row: number;
  readonly column: number;
  readonly orientation: ShipOrientation;
}

interface BoardCellViewModel {
  readonly key: string;
  readonly row: number;
  readonly column: number;
  readonly label: string;
  readonly occupiedBy: string | null;
}

interface InventoryShipViewModel extends ShipDefinition {
  readonly isPlaced: boolean;
  readonly statusLabel: string;
}

interface PlacedShipViewModel extends ShipDefinition, ShipPlacement {
  readonly gridColumn: string;
  readonly gridRow: string;
  readonly occupiedCellsLabel: string;
}

const boardDimension = 10;
const rowLabels = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'] as const;
const shipDefinitions: readonly ShipDefinition[] = [
  { id: 'carrier', label: 'Carrier', length: 5, accent: 'surge' },
  { id: 'battleship', label: 'Battleship', length: 4, accent: 'amber' },
  { id: 'cruiser', label: 'Cruiser', length: 3, accent: 'soft' },
  { id: 'submarine', label: 'Submarine', length: 3, accent: 'surge' },
  { id: 'destroyer', label: 'Destroyer', length: 2, accent: 'damage' }
];

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
  private readonly identityService = inject(AnonymousPlayerIdentityService);

  protected readonly gameCode = inject(ActivatedRoute).snapshot.paramMap.get('gameCode') ?? '';
  protected readonly boardDimension = boardDimension;
  protected readonly boardColumns = Array.from({ length: boardDimension }, (_, index) => index + 1);
  protected readonly boardCells = computed(() => this.buildBoardCells());
  protected readonly fleetLocked = signal(false);
  protected readonly selectedShipId = signal<string | null>(null);
  protected readonly draggingShipId = signal<string | null>(null);
  protected readonly placements = signal<Record<string, ShipPlacement>>({});
  protected readonly currentPlayerName = computed(
    () => this.identityService.session()?.playerName.toUpperCase() ?? 'ANONYMOUS CAPTAIN'
  );
  protected readonly opponentName = computed(() => 'AWAITING OPPONENT');
  protected readonly inventoryShips = computed<readonly InventoryShipViewModel[]>(() => {
    const placements = this.placements();

    return shipDefinitions.map((ship) => ({
      ...ship,
      isPlaced: placements[ship.id] !== undefined,
      statusLabel: placements[ship.id] === undefined ? 'Awaiting deployment' : 'Deployed on the grid'
    }));
  });
  protected readonly allShipsPlaced = computed(() =>
    shipDefinitions.every((ship) => this.placements()[ship.id] !== undefined)
  );
  protected readonly readyActionVisible = computed(() => this.allShipsPlaced() && !this.fleetLocked());
  protected readonly stateHeading = computed(() => {
    if (this.fleetLocked()) {
      return 'FLEET LOCKED';
    }

    return this.allShipsPlaced() ? 'READY TO TRANSMIT' : 'SETUP IN PROGRESS';
  });
  protected readonly stateDetail = computed(() => {
    if (this.fleetLocked()) {
      return 'All hulls are fixed in place. Movement and rotation controls are offline.';
    }

    return this.allShipsPlaced()
      ? 'Every hull is seated on valid cells. Confirm readiness to lock the fleet.'
      : 'Drag or select a ship, then drop it onto the board to continue deployment.';
  });
  protected readonly boardInstruction = computed(() => {
    if (this.fleetLocked()) {
      return 'Fleet positions are locked and awaiting the opposing commander.';
    }

    const selectedShip = this.selectedShip();

    if (selectedShip !== null) {
      return `${selectedShip.label.toUpperCase()} selected // deploy or reposition on a valid grid cell.`;
    }

    return 'Select a hull from the inventory or drag one straight onto the setup board.';
  });
  protected readonly placedShips = computed<readonly PlacedShipViewModel[]>(() => {
    const placements = this.placements();

    return shipDefinitions.flatMap((ship) => {
      const placement = placements[ship.id];

      if (placement === undefined) {
        return [];
      }

      const rowSpan = placement.orientation === 'vertical' ? ship.length : 1;
      const columnSpan = placement.orientation === 'horizontal' ? ship.length : 1;

      return [
        {
          ...ship,
          ...placement,
          gridColumn: `${placement.column + 1} / span ${columnSpan}`,
          gridRow: `${placement.row + 1} / span ${rowSpan}`,
          occupiedCellsLabel: this.describePlacement(placement, ship.length)
        }
      ];
    });
  });
  protected readonly selectedPlacedShip = computed<PlacedShipViewModel | null>(() => {
    const selectedShipId = this.selectedShipId();

    if (selectedShipId === null) {
      return null;
    }

    return this.placedShips().find((ship) => ship.id === selectedShipId) ?? null;
  });

  protected selectShip(shipId: string): void {
    this.selectedShipId.set(shipId);
  }

  protected selectPlacedShip(shipId: string, event?: Event): void {
    event?.stopPropagation();
    this.selectedShipId.set(shipId);
  }

  protected deploySelectedShip(row: number, column: number): void {
    if (this.fleetLocked()) {
      return;
    }

    const selectedShipId = this.selectedShipId();

    if (selectedShipId === null) {
      return;
    }

    this.commitPlacement(selectedShipId, row, column);
  }

  protected allowBoardDrop(event: DragEvent): void {
    if (this.fleetLocked()) {
      return;
    }

    event.preventDefault();
  }

  protected handleBoardDrop(event: DragEvent, row: number, column: number): void {
    event.preventDefault();

    if (this.fleetLocked()) {
      return;
    }

    const shipId = event.dataTransfer?.getData('text/plain') || this.draggingShipId() || this.selectedShipId();

    if (shipId === null || shipId === '') {
      return;
    }

    this.commitPlacement(shipId, row, column);
  }

  protected handleShipDragStart(event: DragEvent, shipId: string): void {
    if (this.fleetLocked()) {
      event.preventDefault();
      return;
    }

    this.draggingShipId.set(shipId);
    this.selectedShipId.set(shipId);
    event.dataTransfer?.setData('text/plain', shipId);

    if (event.dataTransfer !== null) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  protected handleShipDragEnd(): void {
    this.draggingShipId.set(null);
  }

  protected rotateSelectedShip(): void {
    if (this.fleetLocked()) {
      return;
    }

    const selectedShip = this.selectedPlacedShip();

    if (selectedShip === null) {
      return;
    }

    const nextOrientation: ShipOrientation =
      selectedShip.orientation === 'horizontal' ? 'vertical' : 'horizontal';
    const candidate = this.clampPlacement(
      {
        row: selectedShip.row,
        column: selectedShip.column,
        orientation: nextOrientation
      },
      selectedShip.length
    );

    if (!this.isPlacementValid(selectedShip.id, selectedShip.length, candidate)) {
      return;
    }

    this.placements.update((placements) => ({
      ...placements,
      [selectedShip.id]: candidate
    }));
  }

  protected confirmReady(): void {
    if (!this.allShipsPlaced() || this.fleetLocked()) {
      return;
    }

    this.fleetLocked.set(true);
    this.selectedShipId.set(null);
    this.draggingShipId.set(null);
  }

  private selectedShip(): ShipDefinition | null {
    const selectedShipId = this.selectedShipId();

    if (selectedShipId === null) {
      return null;
    }

    return shipDefinitions.find((ship) => ship.id === selectedShipId) ?? null;
  }

  private commitPlacement(shipId: string, row: number, column: number): void {
    const ship = shipDefinitions.find((entry) => entry.id === shipId);

    if (ship === undefined) {
      return;
    }

    const currentPlacement = this.placements()[shipId];
    const orientation = currentPlacement?.orientation ?? 'horizontal';
    const candidate = this.clampPlacement({ row, column, orientation }, ship.length);

    if (!this.isPlacementValid(shipId, ship.length, candidate)) {
      this.draggingShipId.set(null);
      return;
    }

    this.placements.update((placements) => ({
      ...placements,
      [shipId]: candidate
    }));
    this.selectedShipId.set(shipId);
    this.draggingShipId.set(null);
  }

  private buildBoardCells(): readonly BoardCellViewModel[] {
    const occupiedCells = new Map<string, string>();

    for (const ship of shipDefinitions) {
      const placement = this.placements()[ship.id];

      if (placement === undefined) {
        continue;
      }

      for (const cell of this.getOccupiedCells(placement, ship.length)) {
        occupiedCells.set(this.cellKey(cell.row, cell.column), ship.id);
      }
    }

    return Array.from({ length: boardDimension * boardDimension }, (_, index) => {
      const row = Math.floor(index / boardDimension);
      const column = index % boardDimension;
      const key = this.cellKey(row, column);

      return {
        key,
        row,
        column,
        label: `${rowLabels[row]}${column + 1}`,
        occupiedBy: occupiedCells.get(key) ?? null
      };
    });
  }

  private getOccupiedCells(placement: ShipPlacement, length: number): Array<{ row: number; column: number }> {
    return Array.from({ length }, (_, offset) => ({
      row: placement.row + (placement.orientation === 'vertical' ? offset : 0),
      column: placement.column + (placement.orientation === 'horizontal' ? offset : 0)
    }));
  }

  private isPlacementValid(shipId: string, shipLength: number, placement: ShipPlacement): boolean {
    for (const cell of this.getOccupiedCells(placement, shipLength)) {
      if (cell.row < 0 || cell.row >= boardDimension || cell.column < 0 || cell.column >= boardDimension) {
        return false;
      }
    }

    const candidateCells = new Set(
      this.getOccupiedCells(placement, shipLength).map((cell) => this.cellKey(cell.row, cell.column))
    );

    for (const ship of shipDefinitions) {
      if (ship.id === shipId) {
        continue;
      }

      const existingPlacement = this.placements()[ship.id];

      if (existingPlacement === undefined) {
        continue;
      }

      for (const cell of this.getOccupiedCells(existingPlacement, ship.length)) {
        if (candidateCells.has(this.cellKey(cell.row, cell.column))) {
          return false;
        }
      }
    }

    return true;
  }

  private clampPlacement(placement: ShipPlacement, shipLength: number): ShipPlacement {
    const rowLimit = placement.orientation === 'vertical' ? boardDimension - shipLength : boardDimension - 1;
    const columnLimit = placement.orientation === 'horizontal' ? boardDimension - shipLength : boardDimension - 1;

    return {
      row: Math.min(Math.max(placement.row, 0), rowLimit),
      column: Math.min(Math.max(placement.column, 0), columnLimit),
      orientation: placement.orientation
    };
  }

  private describePlacement(placement: ShipPlacement, shipLength: number): string {
    return this.getOccupiedCells(placement, shipLength)
      .map((cell) => `${rowLabels[cell.row]}${cell.column + 1}`)
      .join(', ');
  }

  private cellKey(row: number, column: number): string {
    return `${row}-${column}`;
  }
}
