import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/public/home/landing-page/landing-page').then((module) => module.LandingPage)
  }
];
