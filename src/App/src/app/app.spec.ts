import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  const originalAudio = window.Audio;
  let audioInstances: FakeAudio[];

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-motion');
    document.documentElement.removeAttribute('data-density');
    document.documentElement.removeAttribute('data-accent');
    audioInstances = [];
    window.Audio = class extends FakeAudio {
      constructor(src?: string) {
        super(src);
        audioInstances.push(this);
      }
    } as unknown as typeof Audio;
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)]
    }).compileComponents();
  });

  afterEach(() => {
    window.Audio = originalAudio;
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the landing page at the root route', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('BATTLE OPS // BELOW THE SURFACE');
    expect(compiled.textContent).toContain('TACTICAL TERMINAL');
  });

  it('should save default sound volumes locally on first load', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(localStorage.getItem('battle-ops-effects-volume')).toBe('50');
    expect(localStorage.getItem('battle-ops-music-volume')).toBe('30');
    expect(audioInstances[0].src).toContain('audio/background-song-01.mp3');
    expect(audioInstances[0].volume).toBe(0.3);
  });

  it('should focus the home page on playing Battleship', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;

    expect(host.textContent).not.toContain('TACTICAL SETTINGS');
    expect(host.textContent).not.toContain('GAME MODE');
    expect(host.textContent).not.toContain('BOARD SIZE');
    expect(host.textContent).not.toContain('PLAYER STATUS');
    expect(host.textContent).toContain('PLAY BATTLESHIP');
    expect(host.textContent).toContain('teach AI-driven development');
    expect(host.textContent).toContain('Aspire orchestration');
    expect(host.querySelectorAll('.vu-meter-card').length).toBe(3);
  });

  it('should open sound settings and persist sound volumes locally', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    const soundToggle = host.querySelector('.sound-settings-toggle') as HTMLButtonElement | null;

    soundToggle?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const effectsSlider = host.querySelector('#effects-volume') as HTMLInputElement | null;
    const musicSlider = host.querySelector('#music-volume') as HTMLInputElement | null;

    effectsSlider!.value = '55';
    effectsSlider!.dispatchEvent(new Event('input'));
    musicSlider!.value = '70';
    musicSlider!.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(host.textContent).toContain('SOUND SETTINGS');
    expect(localStorage.getItem('battle-ops-effects-volume')).toBe('55');
    expect(localStorage.getItem('battle-ops-music-volume')).toBe('70');
    expect(host.textContent).toContain('rotates between both Battle Ops music tracks');
    expect(audioInstances[0].volume).toBe(0.7);
  });

  it('should start the soundtrack after the first interaction and rotate tracks', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    document.dispatchEvent(new Event('pointerdown'));
    await Promise.resolve();

    expect(audioInstances[0].playCalls).toBe(1);

    audioInstances[0].dispatch('ended');
    await Promise.resolve();

    expect(audioInstances[0].src).toContain('audio/background-song-02.mp3');
    expect(audioInstances[0].loadCalls).toBe(1);
    expect(audioInstances[0].playCalls).toBe(2);
  });

  it('should echo visitor terminal input into the landing page terminal', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    const terminalInput = host.querySelector('#terminal-command') as HTMLInputElement | null;
    const submitButton = host.querySelector('.terminal-input__row button') as HTMLButtonElement | null;

    terminalInput!.value = 'hello captain';
    terminalInput!.dispatchEvent(new Event('input'));
    submitButton?.click();
    fixture.detectChanges();
    await new Promise((resolve) => window.setTimeout(resolve, 600));
    fixture.detectChanges();

    expect(host.textContent).toContain('> HELLO CAPTAIN');
  });
});

class FakeAudio {
  currentTime = 0;
  loop = false;
  paused = true;
  preload = '';
  src: string;
  volume = 1;
  playCalls = 0;
  loadCalls = 0;
  private readonly listeners = new Map<string, Array<() => void>>();

  constructor(src?: string) {
    this.src = src ?? '';
  }

  addEventListener(type: string, listener: () => void): void {
    const listeners = this.listeners.get(type) ?? [];
    listeners.push(listener);
    this.listeners.set(type, listeners);
  }

  removeEventListener(type: string, listener: () => void): void {
    const listeners = this.listeners.get(type) ?? [];
    this.listeners.set(
      type,
      listeners.filter((entry) => entry !== listener)
    );
  }

  load(): void {
    this.loadCalls += 1;
  }

  pause(): void {
    this.paused = true;
  }

  play(): Promise<void> {
    this.playCalls += 1;
    this.paused = false;
    return Promise.resolve();
  }

  dispatch(type: string): void {
    if (type === 'ended') {
      this.paused = true;
    }

    for (const listener of this.listeners.get(type) ?? []) {
      listener();
    }
  }
}
