import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { ShellComponent } from './shared/shell/shell.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'devices', pathMatch: 'full' },
      { path: 'users',   loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent) },
      { path: 'tenants', loadComponent: () => import('./features/tenants/tenants.component').then(m => m.TenantsComponent) },
      { path: 'sensors', loadComponent: () => import('./features/sensors/sensors.component').then(m => m.SensorsComponent) },
      { path: 'devices', loadComponent: () => import('./features/devices/device-list/device-list.component').then(m => m.DeviceListComponent) },
      { path: 'devices/register', loadComponent: () => import('./features/devices/device-register/device-register.component').then(m => m.DeviceRegisterComponent) },
      { path: 'devices/:id/sensors',   loadComponent: () => import('./features/devices/device-sensors/device-sensors.component').then(m => m.DeviceSensorsComponent) },
      { path: 'devices/:id/scenarios', loadComponent: () => import('./features/devices/device-scenarios/device-scenarios.component').then(m => m.DeviceScenariosComponent) },
      { path: 'geo-sync', loadComponent: () => import('./features/geo-sync/geo-sync.component').then(m => m.GeoSyncComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
