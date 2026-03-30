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

  it('should persist style settings locally', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    const motionToggle = host.querySelector('input[type="checkbox"]') as HTMLInputElement | null;
    const densityButtons = host.querySelectorAll('.option-chip');
    const relaxedDensityButton = densityButtons.item(2) as HTMLButtonElement | null;

    motionToggle?.click();
    relaxedDensityButton?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(localStorage.getItem('battle-ops-reduced-motion')).toBe('true');
    expect(localStorage.getItem('battle-ops-density')).toBe('relaxed');
    expect(document.documentElement.dataset['motion']).toBe('reduced');
    expect(document.documentElement.dataset['density']).toBe('relaxed');
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
