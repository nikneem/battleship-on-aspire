import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, input, signal } from '@angular/core';

@Component({
  selector: 'bat-landing-terminal',
  imports: [],
  templateUrl: './landing-terminal.html',
  styleUrl: './landing-terminal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingTerminal {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private typingTimer: number | null = null;

  readonly lines = input<readonly string[]>([]);
  readonly visitorInput = signal('');
  readonly visitorMessages = signal<string[]>([]);
  readonly combinedLines = computed(() => [...this.lines(), ...this.visitorMessages()]);
  readonly historyLines = computed(() => this.combinedLines().slice(-6, -1).reverse());
  readonly activeLine = computed(() => this.combinedLines().at(-1) ?? 'SCANNING FOR CONTACTS...');
  readonly typedLine = signal('');

  constructor() {
    effect(() => {
      this.typeLine(this.activeLine());
    });

    this.destroyRef.onDestroy(() => {
      this.clearTypingTimer();
    });
  }

  private typeLine(line: string): void {
    const view = this.document.defaultView;

    this.clearTypingTimer();

    if (view === null) {
      this.typedLine.set(line);
      return;
    }

    this.typedLine.set('');
    let cursor = 0;

    const writeNextCharacter = (): void => {
      cursor += 1;
      this.typedLine.set(line.slice(0, cursor));

      if (cursor < line.length) {
        this.typingTimer = view.setTimeout(writeNextCharacter, 18);
      }
    };

    this.typingTimer = view.setTimeout(writeNextCharacter, 140);
  }

  protected updateVisitorInput(event: Event): void {
    const target = event.target;

    if (target instanceof HTMLInputElement) {
      this.visitorInput.set(target.value);
    }
  }

  protected submitVisitorMessage(): void {
    const normalizedMessage = this.visitorInput().trim();

    if (normalizedMessage.length === 0) {
      return;
    }

    this.visitorMessages.update((messages) => [...messages.slice(-4), `> ${normalizedMessage.toUpperCase()}`]);
    this.visitorInput.set('');
  }

  private clearTypingTimer(): void {
    const view = this.document.defaultView;

    if (view !== null && this.typingTimer !== null) {
      view.clearTimeout(this.typingTimer);
    }

    this.typingTimer = null;
  }
}
