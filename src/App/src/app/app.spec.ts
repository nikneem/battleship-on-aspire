import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-motion');
    document.documentElement.removeAttribute('data-density');
    document.documentElement.removeAttribute('data-accent');
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)]
    }).compileComponents();
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
  });

  it('should focus the home page on playing Battleship', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;

    expect(host.textContent).not.toContain('TACTICAL SETTINGS');
    expect(host.textContent).toContain('PLAY BATTLESHIP');
    expect(host.textContent).toContain('playable Battleship experience');
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
