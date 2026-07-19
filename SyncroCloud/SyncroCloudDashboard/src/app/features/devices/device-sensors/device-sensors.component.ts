import { Component, OnInit, computed, inject, signal, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DeviceService } from '../../../core/services/device.service';
import { SensorService } from '../../../core/services/sensor.service';
import { DeviceSensorDto } from '../../../core/models/device.models';
import { SensorDto } from '../../../core/models/sensor.models';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { sortRows } from '../../../shared/utils/table-sort';
import { InstallSensorDialogComponent, InstallSensorDialogData, InstallSensorDialogResult } from './install-sensor-dialog/install-sensor-dialog.component';
import { EditSensorDialogComponent, EditSensorDialogData, EditSensorDialogResult } from './edit-sensor-dialog/edit-sensor-dialog.component';

@Component({
  selector: 'app-device-sensors',
  standalone: true,
  imports: [RouterLink, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatDialogModule],
  templateUrl: './device-sensors.component.html',
  styleUrl: './device-sensors.component.scss'
})
export class DeviceSensorsComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  auth = inject(AuthService);
  private sensorService = inject(SensorService);
  private dialog         = inject(MatDialog);
  private cdr           = inject(ChangeDetectorRef);

  deviceId!: string;
  installed = signal<DeviceSensorDto[]>([]);
  availableSensors: SensorDto[]       = [];
  columns = ['sensor', 'switchNo', 'unitId', 'displayName', 'sensorType', 'actions'];

  sort = signal<Sort>({ active: '', direction: '' });
  sortedInstalled = computed(() =>
    sortRows(this.installed(), this.sort(), (s, col) => this.sortValue(s, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(s: DeviceSensorDto, col: string): unknown {
    switch (col) {
      case 'sensor':      return this.sensorName(s.sensorId);
      case 'switchNo':    return s.switchNo;
      case 'unitId':      return s.unitId;
      case 'displayName': return s.displayName;
      case 'sensorType':  return s.sensorType;
      default:            return '';
    }
  }

  ngOnInit() {
    this.deviceId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    forkJoin([
      this.deviceService.getSensors(this.deviceId),
      this.sensorService.getAll()
    ]).subscribe({
      next: ([installed, sensors]) => {
        this.availableSensors = sensors;
        this.installed.set(installed);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load sensors page data', err)
    });
  }

  sensorName(sensorId: string) {
    return this.availableSensors.find(s => s.sensorId === sensorId)?.name ?? sensorId;
  }

  openInstallDialog() {
    const ref = this.dialog.open<InstallSensorDialogComponent, InstallSensorDialogData, InstallSensorDialogResult>(
      InstallSensorDialogComponent,
      {
        width: '600px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { availableSensors: this.availableSensors }
      }
    );

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.install(result);
    });
  }

  private install(result: InstallSensorDialogResult) {
    const dto = {
      sensorId:    result.sensorId,
      switchNo:    result.switchNo,
      unitId:      result.unitId,
      displayName: result.displayName,
      address:     result.address,
      port:        result.port,
      url:              result.url,
      deviceId:         this.deviceId,
      sensorType:       result.selectedSensor.type,
      protocol:         0,
      baseUrl:          result.selectedSensor.baseUrl,
      portNo:           result.selectedSensor.portNo,
      dataPath:         result.selectedSensor.dataPath,
      infoPath:         result.selectedSensor.infoPath,
      inchingPath:      result.selectedSensor.inchingPath,
      syncPeriodicity:  result.selectedSensor.syncPeriodicity,
      eventChangeSync:  result.selectedSensor.eventChangeSync,
      eventChangeDelta: result.selectedSensor.eventChangeDelta,
      installedById:    null
    } as any;

    this.deviceService.installSensor(dto).subscribe({
      next: () => this.load(),
      error: (err) => console.error('Failed to install sensor', err)
    });
  }

  openEditDialog(sensor: DeviceSensorDto) {
    const ref = this.dialog.open<EditSensorDialogComponent, EditSensorDialogData, EditSensorDialogResult>(
      EditSensorDialogComponent,
      {
        width: '420px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { sensor }
      }
    );

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.update(sensor, result);
    });
  }

  private update(sensor: DeviceSensorDto, result: EditSensorDialogResult) {
    const dto = {
      switchNo:               sensor.switchNo,
      unitId:                 sensor.unitId,
      address:                result.address,
      port:                   result.port,
      displayName:            result.displayName,
      url:                    sensor.url,
      sensorType:             sensor.sensorType,
      protocol:               sensor.protocol,
      dataPath:               sensor.dataPath,
      infoPath:               sensor.infoPath,
      inchingPath:            sensor.inchingPath,
      syncPeriodicity:        sensor.syncPeriodicity,
      eventChangeSync:        sensor.eventChangeSync,
      eventChangeDelta:       sensor.eventChangeDelta,
      onlySaveRecordOnChange: sensor.onlySaveRecordOnChange,
      isInInchingMode:        sensor.isInInchingMode,
      inchingModeWidthInMs:   sensor.inchingModeWidthInMs,
      isActive:               sensor.isActive,
      notes:                  sensor.notes
    };

    this.deviceService.updateSensor(sensor.id, dto).subscribe({
      next: () => this.load(),
      error: (err) => console.error('Failed to update sensor', err)
    });
  }

  uninstall(id: string) {
    if (confirm('Uninstall this sensor?')) {
      this.deviceService.uninstallSensor(id).subscribe({
        next: () => this.load(),
        error: (err) => console.error('Failed to uninstall sensor', err)
      });
    }
  }
}
