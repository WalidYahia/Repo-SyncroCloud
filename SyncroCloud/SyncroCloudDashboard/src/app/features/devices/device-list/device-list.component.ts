import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DeviceService } from '../../../core/services/device.service';
import { TenantService } from '../../../core/services/tenant.service';
import { DeviceDto } from '../../../core/models/device.models';
import { TenantDto } from '../../../core/models/tenant.models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [RouterLink, DatePipe, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.scss'
})
export class DeviceListComponent implements OnInit {
  private deviceService = inject(DeviceService);
  private tenantService = inject(TenantService);

  devices: DeviceDto[] = [];
  tenants: TenantDto[] = [];
  columns = ['name', 'deviceId', 'tenant', 'type', 'status', 'lastSeen', 'actions'];
  loading = signal(true);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    forkJoin({
      devices: this.deviceService.getAll(),
      tenants: this.tenantService.getAll()
    }).subscribe({
      next: ({ devices, tenants }) => {
        this.devices = devices;
        this.tenants = tenants;
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); }
    });
  }

  tenantName(id: string) { return this.tenants.find(t => t.id === id)?.name ?? id; }

  statusColor(s: string) { return s === 'Online' ? 'primary' : s === 'Maintenance' ? 'accent' : 'warn'; }

  delete(id: string) {
    if (confirm('Delete this device?')) {
      this.deviceService.delete(id).subscribe(() => this.load());
    }
  }
}
