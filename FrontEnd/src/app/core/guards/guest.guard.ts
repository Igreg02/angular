import { CanActivateFn,Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';

export const guestGuard: CanActivateFn = (route, state) => {

  const authservice = inject(AuthService);
  const router      = inject(Router);
  if(!authservice.isAuthenticated())
  {
    return true
  }
  return router.createUrlTree(["/dashboard"]);
};
