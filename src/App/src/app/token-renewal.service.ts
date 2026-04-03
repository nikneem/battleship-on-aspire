import { effect, inject, Injectable, OnDestroy } from '@angular/core';
import { Subscription, timer } from 'rxjs';

import {
  AnonymousPlayerIdentity,
  AnonymousPlayerIdentityService
} from './anonymous-player-identity.service';

/**
 * How many milliseconds before token expiry to trigger renewal.
 * Must be less than the server-side RenewalWindow (5 minutes = 300 000 ms).
 * Targeting 4.5 minutes gives a comfortable window while staying well within expiry.
 */
const RENEWAL_TRIGGER_MS = 4.5 * 60 * 1_000;

@Injectable({ providedIn: 'root' })
export class TokenRenewalService implements OnDestroy {
  private readonly identityService = inject(AnonymousPlayerIdentityService);
  private renewalSubscription: Subscription | null = null;

  constructor() {
    effect(() => {
      const session = this.identityService.session();
      this.schedule(session);
    });
  }

  ngOnDestroy(): void {
    this.cancel();
  }

  private schedule(session: AnonymousPlayerIdentity | null): void {
    this.cancel();

    if (!session) {
      return;
    }

    const expiresAtMs = new Date(session.expiresAtUtc).getTime();
    const triggerAtMs = expiresAtMs - RENEWAL_TRIGGER_MS;
    const delayMs = Math.max(0, triggerAtMs - Date.now());

    this.renewalSubscription = timer(delayMs).subscribe(() => {
      const current = this.identityService.session();
      if (current) {
        this.renew(current.accessToken);
      }
    });
  }

  private renew(accessToken: string): void {
    this.identityService.renewSession(accessToken).subscribe({
      error: (err) => console.error('[TokenRenewalService] Token renewal failed:', err)
    });
  }

  private cancel(): void {
    this.renewalSubscription?.unsubscribe();
    this.renewalSubscription = null;
  }
}
