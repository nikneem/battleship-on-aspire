import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameOutcomeOverlayComponent } from './game-outcome-overlay';

@Component({
  selector: 'test-host-winner',
  imports: [GameOutcomeOverlayComponent],
  template: `<bat-game-outcome-overlay [outcome]="outcome" (backToMain)="onBack()" />`
})
class TestHostWinnerComponent {
  outcome: 'winner' | 'loser' = 'winner';
  backed = false;
  onBack(): void { this.backed = true; }
}

@Component({
  selector: 'test-host-loser',
  imports: [GameOutcomeOverlayComponent],
  template: `<bat-game-outcome-overlay [outcome]="outcome" (backToMain)="onBack()" />`
})
class TestHostLoserComponent {
  outcome: 'winner' | 'loser' = 'loser';
  backed = false;
  onBack(): void { this.backed = true; }
}

describe('GameOutcomeOverlayComponent', () => {
  describe('when outcome is winner', () => {
    let fixture: ComponentFixture<TestHostWinnerComponent>;
    let host: HTMLElement;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [GameOutcomeOverlayComponent, TestHostWinnerComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(TestHostWinnerComponent);
      fixture.detectChanges();
      host = fixture.nativeElement as HTMLElement;
    });

    it('should render WINNER text', () => {
      expect(host.textContent).toContain('WINNER!');
    });

    it('should not render YOU LOST text', () => {
      expect(host.textContent).not.toContain('YOU LOST');
    });

    it('should render the winner icon class', () => {
      const icon = host.querySelector('.outcome-icon');
      expect(icon?.classList).toContain('winner');
      expect(icon?.classList).not.toContain('loser');
    });

    it('should emit backToMain when back button is clicked', () => {
      const button = host.querySelector('.back-btn') as HTMLButtonElement;
      button.click();
      fixture.detectChanges();
      expect(fixture.componentInstance.backed).toBe(true);
    });
  });

  describe('when outcome is loser', () => {
    let fixture: ComponentFixture<TestHostLoserComponent>;
    let host: HTMLElement;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [GameOutcomeOverlayComponent, TestHostLoserComponent]
      }).compileComponents();

      fixture = TestBed.createComponent(TestHostLoserComponent);
      fixture.detectChanges();
      host = fixture.nativeElement as HTMLElement;
    });

    it('should render YOU LOST text', () => {
      expect(host.textContent).toContain('YOU LOST');
    });

    it('should not render WINNER text', () => {
      expect(host.textContent).not.toContain('WINNER!');
    });

    it('should render the loser icon class', () => {
      const icon = host.querySelector('.outcome-icon');
      expect(icon?.classList).toContain('loser');
      expect(icon?.classList).not.toContain('winner');
    });

    it('should emit backToMain when back button is clicked', () => {
      const button = host.querySelector('.back-btn') as HTMLButtonElement;
      button.click();
      fixture.detectChanges();
      expect(fixture.componentInstance.backed).toBe(true);
    });
  });
});
