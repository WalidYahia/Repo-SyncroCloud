import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { privilegeGuard, roleGuard } from './core/guards/privilege.guard';
import { ShellComponent } from './shared/shell/shell.component';
import { PrivilegeCodes } from './core/identity/privilege-codes';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'devices', pathMatch: 'full' },

      // Always accessible (all logged-in users see their assigned devices)
      { path: 'devices', loadComponent: () => import('./features/devices/device-list/device-list.component').then(m => m.DeviceListComponent) },

      // Privilege-guarded pages
      { path: 'users',   canActivate: [privilegeGuard(PrivilegeCodes.CreateEditUser)],           loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent) },
      { path: 'roles',   canActivate: [privilegeGuard(PrivilegeCodes.ManageRoles)],               loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent) },
      { path: 'tenants', canActivate: [privilegeGuard(PrivilegeCodes.ManageTenants)],             loadComponent: () => import('./features/tenants/tenants.component').then(m => m.TenantsComponent) },
      { path: 'sensors', canActivate: [privilegeGuard(PrivilegeCodes.DefineSensor)],              loadComponent: () => import('./features/sensors/sensors.component').then(m => m.SensorsComponent) },
      { path: 'geo-sync',canActivate: [roleGuard('SuperAdmin')],                                   loadComponent: () => import('./features/geo-sync/geo-sync.component').then(m => m.GeoSyncComponent) },

      { path: 'devices/register', canActivate: [privilegeGuard(PrivilegeCodes.CreateDevice)],     loadComponent: () => import('./features/devices/device-register/device-register.component').then(m => m.DeviceRegisterComponent) },
      { path: 'devices/:id/sensors',   canActivate: [privilegeGuard(PrivilegeCodes.ManageSensorToDevice)],   loadComponent: () => import('./features/devices/device-sensors/device-sensors.component').then(m => m.DeviceSensorsComponent) },
      { path: 'devices/:id/scenarios', canActivate: [privilegeGuard(PrivilegeCodes.ManageScenarioToDevice)], loadComponent: () => import('./features/devices/device-scenarios/device-scenarios.component').then(m => m.DeviceScenariosComponent) },
      { path: 'users/:id/devices',     canActivate: [privilegeGuard(PrivilegeCodes.AssignDeviceSensorToUser)], loadComponent: () => import('./features/users/user-device-access/user-device-access.component').then(m => m.UserDeviceAccessComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
