import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';

import { AnonymousPlayerIdentityService } from './anonymous-player-identity.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const session = inject(AnonymousPlayerIdentityService).session();

  if (session && req.url.startsWith('/api/')) {
    return next(
      req.clone({
        setHeaders: { Authorization: `Bearer ${session.accessToken}` }
      })
    );
  }

  return next(req);
};
