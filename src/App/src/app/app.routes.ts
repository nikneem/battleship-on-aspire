import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/public/home/landing-page/landing-page').then((module) => module.LandingPage)
  },
  {
    path: 'create-game',
    loadComponent: () =>
      import('./pages/public/games/create-game-page/create-game-page').then((module) => module.CreateGamePage)
  },
  {
    path: 'games/:gameCode',
    loadComponent: () =>
      import('./pages/public/games/game-route-shell/game-route-shell').then((module) => module.GameRouteShell)
  }
];
