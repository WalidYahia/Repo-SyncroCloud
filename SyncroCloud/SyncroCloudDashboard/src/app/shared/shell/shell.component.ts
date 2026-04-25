import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  auth   = inject(AuthService);
  router = inject(Router);

  navItems = [
    { label: 'Devices',  icon: 'router',   path: '/devices'   },
    { label: 'Sensors',  icon: 'sensors',  path: '/sensors'   },
    { label: 'Tenants',  icon: 'business', path: '/tenants'   },
    { label: 'Users',    icon: 'group',    path: '/users'     },
    { label: 'Geo Sync', icon: 'public',   path: '/geo-sync'  },
  ];

  activeLabel = 'SyncroCloud';

  constructor() {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe((e: NavigationEnd) => {
      const match = this.navItems.find(n => e.urlAfterRedirects.startsWith(n.path));
      this.activeLabel = match?.label ?? 'SyncroCloud';
    });
  }
}
