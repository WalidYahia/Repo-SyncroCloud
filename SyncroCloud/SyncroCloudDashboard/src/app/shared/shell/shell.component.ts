import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { PrivilegeCodes } from '../../core/identity/privilege-codes';
import { filter } from 'rxjs';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  privilege?: string;
  role?: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit {
  auth   = inject(AuthService);
  router = inject(Router);

  private readonly allNavItems: NavItem[] = [
    { label: 'Devices',  icon: 'router',           path: '/devices'   },
    { label: 'Sensors',  icon: 'sensors',           path: '/sensors',  privilege: PrivilegeCodes.DefineSensor },
    { label: 'Tenants',  icon: 'business',          path: '/tenants',  privilege: PrivilegeCodes.ManageTenants },
    { label: 'Users',    icon: 'group',             path: '/users',    privilege: PrivilegeCodes.CreateEditUser },
    { label: 'Roles',    icon: 'manage_accounts',   path: '/roles',    privilege: PrivilegeCodes.ManageRoles },
    { label: 'Geo Sync', icon: 'public',            path: '/geo-sync', role: 'SuperAdmin' },
  ];

  get navItems(): NavItem[] {
    return this.allNavItems.filter(item => {
      if (item.role)      return this.auth.hasRole(item.role);
      if (item.privilege) return this.auth.hasPrivilege(item.privilege);
      return true;
    });
  }

  activeLabel = 'SyncroCloud';

  ngOnInit() {
    // Restore profile from localStorage on page refresh (token already exists)
    this.auth.ensureProfile();
  }

  constructor() {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe((e: NavigationEnd) => {
      const match = this.allNavItems.find(n => e.urlAfterRedirects.startsWith(n.path));
      this.activeLabel = match?.label ?? 'SyncroCloud';
    });
  }
}
