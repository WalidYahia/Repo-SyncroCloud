import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin, of } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { UserService } from '../../../core/services/user.service';
import { DeviceService } from '../../../core/services/device.service';
import { UserDto } from '../../../core/models/user.models';
import { DeviceDto, SensorPermissionDto } from '../../../core/models/device.models';
import { sortRows } from '../../../shared/utils/table-sort';
import { AssignDeviceDialogComponent, AssignDeviceDialogData } from './assign-device-dialog/assign-device-dialog.component';
import { SensorPermissionsDialogComponent, SensorPermissionsDialogData } from './sensor-permissions-dialog/sensor-permissions-dialog.component';

@Component({
  selector: 'app-user-device-access',
  standalone: true,
  imports: [RouterLink, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule,
            MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './user-device-access.component.html',
  styleUrl: './user-device-access.component.scss'
})
export class UserDeviceAccessComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  private userService   = inject(UserService);
  private deviceService = inject(DeviceService);
  private dialog         = inject(MatDialog);

  userId!: string;
  user: UserDto | null = null;
  assignedDevices = signal<DeviceDto[]>([]);
  allDevices: DeviceDto[] = [];
  columns = ['name', 'deviceId', 'actions'];
  loading = signal(true);

  sort = signal<Sort>({ active: '', direction: '' });
  sortedDevices = computed(() =>
    sortRows(this.assignedDevices(), this.sort(), (d, col) => this.sortValue(d, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(d: DeviceDto, col: string): unknown {
    switch (col) {
      case 'name':     return d.name;
      case 'deviceId': return d.deviceId;
      default:         return '';
    }
  }

  ngOnInit() {
    this.userId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    this.loading.set(true);

    forkJoin([
      this.userService.getById(this.userId),
      this.deviceService.getByUser(this.userId),
      this.userService.getTenants(this.userId)
    ]).pipe(
      switchMap(([user, assigned, tenants]) => {
        this.user = user;
        this.assignedDevices.set(assigned);

        if (tenants.length === 0) return of([]);

        // Load devices for all of the user's tenants in parallel
        return forkJoin(tenants.map(t => this.deviceService.getByTenant(t.id))).pipe(
          map(arrays => {
            // Deduplicate in case a device appears in multiple tenant results
            const seen = new Set<string>();
            return arrays.flat().filter(d => seen.has(d.deviceId) ? false : !!seen.add(d.deviceId));
          })
        );
      })
    ).subscribe({
      next: tenantDevices => {
        this.allDevices = tenantDevices;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  get availableDevices(): DeviceDto[] {
    const assignedIds = new Set(this.assignedDevices().map(d => d.deviceId));
    return this.allDevices.filter(d => !assignedIds.has(d.deviceId));
  }

  openAssignDialog() {
    const ref = this.dialog.open<AssignDeviceDialogComponent, AssignDeviceDialogData, string>(
      AssignDeviceDialogComponent,
      {
        width: '480px',
        maxWidth: '95vw',
        disableClose: true,
        data: { availableDevices: this.availableDevices }
      }
    );

    ref.afterClosed().subscribe(deviceId => {
      if (!deviceId) return;
      this.deviceService.assignUser(deviceId, this.userId).subscribe(() => this.load());
    });
  }

  openSensorPermissionsDialog(device: DeviceDto) {
    forkJoin([
      this.deviceService.getSensors(device.deviceId),
      this.deviceService.getUserLink(device.deviceId, this.userId)
    ]).subscribe(([sensors, link]) => {
      const ref = this.dialog.open<SensorPermissionsDialogComponent, SensorPermissionsDialogData, SensorPermissionDto[]>(
        SensorPermissionsDialogComponent,
        {
          width: '560px',
          maxWidth: '95vw',
          maxHeight: '90vh',
          disableClose: true,
          data: { device, sensors, existingPermissions: link.sensorPermissions }
        }
      );

      ref.afterClosed().subscribe(permissions => {
        if (!permissions) return;
        this.deviceService.updateSensorPermissions(device.deviceId, this.userId, permissions).subscribe();
      });
    });
  }

  unassign(device: DeviceDto) {
    if (confirm(`Remove "${device.name}" from this user's access?`)) {
      this.deviceService.removeUser(device.deviceId, this.userId).subscribe(() => this.load());
    }
  }
}
