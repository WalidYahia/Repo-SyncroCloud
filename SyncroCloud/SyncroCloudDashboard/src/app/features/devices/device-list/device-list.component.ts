import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DeviceService } from '../../../core/services/device.service';
import { TenantService } from '../../../core/services/tenant.service';
import { AuthService } from '../../../core/services/auth.service';
import { DeviceDto } from '../../../core/models/device.models';
import { TenantDto } from '../../../core/models/tenant.models';
import { sortRows } from '../../../shared/utils/table-sort';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [RouterLink, DatePipe, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.scss'
})
export class DeviceListComponent implements OnInit, OnDestroy {
  private deviceService = inject(DeviceService);
  private tenantService = inject(TenantService);
  auth = inject(AuthService);

  devices = signal<DeviceDto[]>([]);
  tenants = signal<TenantDto[]>([]);
  columns = ['name', 'deviceId', 'tenant', 'type', 'lastSeen', 'actions'];
  loading = signal(true);

  sort = signal<Sort>({ active: '', direction: '' });
  sortedDevices = computed(() =>
    sortRows(this.devices(), this.sort(), (d, col) => this.sortValue(d, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(d: DeviceDto, col: string): unknown {
    switch (col) {
      case 'name':     return d.name;
      case 'deviceId': return d.deviceId;
      case 'tenant':   return this.tenantName(d.tenantId);
      case 'type':     return d.type;
      case 'lastSeen': return d.lastSeenAt ? new Date(d.lastSeenAt).getTime() : null;
      default:         return '';
    }
  }

  /** Drives last-seen colour re-evaluation; bumped on every auto-refresh. */
  private now = signal(Date.now());
  private tick?: ReturnType<typeof setInterval>;

  ngOnInit() {
    this.load();
    // Auto-refresh every 30s so last-seen times/colours stay current without a manual reload.
    this.tick = setInterval(() => this.refresh(), 30_000);
  }

  ngOnDestroy() {
    if (this.tick) clearInterval(this.tick);
  }

  /**
   * Colour bucket for a device's last-seen time:
   *   ≤ 5 min → green, 5–15 min → orange, > 15 min → red, never seen → muted.
   */
  lastSeenClass(lastSeenAt: string | null | undefined): string {
    if (!lastSeenAt) return 'ls-none';
    const minutes = (this.now() - new Date(lastSeenAt).getTime()) / 60_000;
    if (minutes <= 5)  return 'ls-green';
    if (minutes <= 15) return 'ls-orange';
    return 'ls-red';
  }

  load() {
    this.loading.set(true);
    forkJoin({
      devices: this.deviceService.getAll(),
      tenants: this.tenantService.getAll()
    }).subscribe({
      next: ({ devices, tenants }) => {
        this.devices.set(devices);
        this.tenants.set(tenants);
        this.now.set(Date.now());
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); }
    });
  }

  /** Silent background refresh (no spinner) used by the 30s auto-refresh. */
  private refresh() {
    this.deviceService.getAll().subscribe({
      next: devices => {
        this.devices.set(devices);
        this.now.set(Date.now());
      },
      error: () => { /* keep showing last known data on transient errors */ }
    });
  }

  tenantName(id: string) { return this.tenants().find(t => t.id === id)?.name ?? id; }

  delete(id: string) {
    if (confirm('Delete this device?')) {
      this.deviceService.delete(id).subscribe(() => this.load());
    }
  }
}
