import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, input, signal } from '@angular/core';
import { type RadarContact } from '../../landing-page.models';

interface MovingRadarContact {
  readonly id: string;
  readonly pulseDelay: string;
  readonly x: number;
  readonly y: number;
  readonly velocityX: number;
  readonly velocityY: number;
  readonly coordinates: string;
  readonly code: string;
}

@Component({
  selector: 'bat-landing-radar',
  imports: [NgOptimizedImage],
  templateUrl: './landing-radar.html',
  styleUrl: './landing-radar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingRadar {
  private readonly destroyRef = inject(DestroyRef);
  private intervalId: number | null = null;

  readonly contacts = input<readonly RadarContact[]>([]);
  protected readonly movingContacts = signal<readonly MovingRadarContact[]>([]);
  protected readonly renderedContacts = computed(() =>
    this.movingContacts().map((contact) => ({
      id: contact.id,
      top: `${contact.y}%`,
      left: `${contact.x}%`,
      coordinates: contact.coordinates,
      code: contact.code,
      pulseDelay: contact.pulseDelay
    }))
  );

  constructor() {
    effect(() => {
      this.movingContacts.set(this.contacts().map((contact) => this.createMovingContact(contact)));
      this.startAnimationLoop();
    });

    this.destroyRef.onDestroy(() => {
      const view = globalThis.window;

      if (view !== undefined && this.intervalId !== null) {
        view.clearInterval(this.intervalId);
      }
    });
  }

  private startAnimationLoop(): void {
    const view = globalThis.window;

    if (view === undefined || this.intervalId !== null) {
      return;
    }

    this.intervalId = view.setInterval(() => {
      this.movingContacts.update((contacts) => contacts.map((contact) => this.advanceContact(contact)));
    }, 180);
  }

  private advanceContact(contact: MovingRadarContact): MovingRadarContact {
    const nextX = contact.x + contact.velocityX;
    const nextY = contact.y + contact.velocityY;
    const offsetX = nextX - 50;
    const offsetY = nextY - 50;
    const distanceFromCenter = Math.hypot(offsetX, offsetY);

    if (distanceFromCenter > 42) {
      return {
        id: contact.id,
        pulseDelay: contact.pulseDelay,
        ...this.createRandomSpawn()
      };
    }

    return {
      ...contact,
      x: nextX,
      y: nextY
    };
  }

  private createMovingContact(contact: RadarContact): MovingRadarContact {
    const spawn = this.createSpawnFromSeed(contact);

    return {
      id: contact.id,
      pulseDelay: contact.pulseDelay,
      ...spawn
    };
  }

  private createSpawnFromSeed(contact: RadarContact): Omit<MovingRadarContact, 'id' | 'pulseDelay'> {
    const parsedX = Number.parseFloat(contact.left);
    const parsedY = Number.parseFloat(contact.top);
    const angle = this.randomBetween(0, Math.PI * 2);
    const speed = this.randomBetween(0.12, 0.28);

    return {
      x: Number.isFinite(parsedX) ? parsedX : this.randomBetween(20, 80),
      y: Number.isFinite(parsedY) ? parsedY : this.randomBetween(20, 80),
      velocityX: Math.cos(angle) * speed,
      velocityY: Math.sin(angle) * speed,
      coordinates: contact.coordinates,
      code: contact.code
    };
  }

  private createRandomSpawn(): Omit<MovingRadarContact, 'id' | 'pulseDelay'> {
    const distance = this.randomBetween(10, 34);
    const positionAngle = this.randomBetween(0, Math.PI * 2);
    const directionAngle = this.randomBetween(0, Math.PI * 2);
    const speed = this.randomBetween(0.12, 0.28);

    return {
      x: 50 + Math.cos(positionAngle) * distance,
      y: 50 + Math.sin(positionAngle) * distance,
      velocityX: Math.cos(directionAngle) * speed,
      velocityY: Math.sin(directionAngle) * speed,
      coordinates: this.createCoordinates(),
      code: this.createCode()
    };
  }

  private createCoordinates(): string {
    const column = String.fromCharCode(65 + Math.floor(this.randomBetween(0, 8)));
    const row = Math.floor(this.randomBetween(1, 9));
    return `${column}${row}`;
  }

  private createCode(): string {
    const prefixes = ['SABLE', 'EMBER', 'NOVA', 'TRIDENT', 'ABYSS', 'SIREN'] as const;
    const prefix = prefixes[Math.floor(this.randomBetween(0, prefixes.length))];
    const suffix = Math.floor(this.randomBetween(10, 99));
    return `${prefix}-${suffix}`;
  }

  private randomBetween(minimum: number, maximum: number): number {
    return Math.random() * (maximum - minimum) + minimum;
  }
}
