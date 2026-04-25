import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { DeviceService } from '../../../core/services/device.service';
import { SensorService } from '../../../core/services/sensor.service';
import { DeviceSensorDto } from '../../../core/models/device.models';
import { SensorDto } from '../../../core/models/sensor.models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-device-sensors',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
            MatFormFieldModule, MatInputModule, MatSelectModule, MatCardModule, MatTooltipModule, MatChipsModule],
  templateUrl: './device-sensors.component.html',
  styleUrl: './device-sensors.component.scss'
})
export class DeviceSensorsComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  private sensorService = inject(SensorService);
  private fb            = inject(FormBuilder);

  deviceId!: string;
  installed:        DeviceSensorDto[] = [];
  availableSensors: SensorDto[]       = [];
  selectedSensor:   SensorDto | null  = null;
  columns = ['sensor', 'switchNo', 'unitId', 'displayName', 'sensorType', 'actions'];
  showForm = false;

  switchNos = ['Non','Switch1','Switch2','Switch3','Switch4','Switch5','Switch6','Switch7','Switch8'];

  form = this.fb.group({
    sensorId:    ['', Validators.required],
    switchNo:    ['Non'],
    unitId:      ['', Validators.required],
    displayName: ['', Validators.required],
    url:         [''],
    address:     [null as number | null],
    port:        [null as number | null],
  });

  ngOnInit() {
    this.deviceId = this.route.snapshot.paramMap.get('id')!;
    forkJoin([
      this.deviceService.getSensors(this.deviceId),
      this.sensorService.getAll()
    ]).subscribe(([installed, sensors]) => {
      this.installed        = installed;
      this.availableSensors = sensors;
    });
  }

  onSensorChange(sensorId: string) {
    this.selectedSensor = this.availableSensors.find(s => s.sensorId === sensorId) ?? null;
    if (this.selectedSensor) {
      this.form.patchValue({ displayName: this.selectedSensor.name });
    }
  }

  sensorName(sensorId: string) {
    return this.availableSensors.find(s => s.sensorId === sensorId)?.name ?? sensorId;
  }

  install() {
    if (this.form.invalid || !this.selectedSensor) return;
    const dto = {
      ...this.form.value,
      deviceId:         this.deviceId,
      sensorType:       this.selectedSensor.type,
      unitType:         this.selectedSensor.unitType,
      protocol:         0,
      syncPeriodicity:  this.selectedSensor.syncPeriodicity,
      eventChangeSync:  this.selectedSensor.eventChangeSync,
      eventChangeDelta: this.selectedSensor.eventChangeDelta,
      installedById:    null
    } as any;

    this.deviceService.installSensor(dto).subscribe(s => {
      this.installed.push(s);
      this.showForm      = false;
      this.selectedSensor = null;
      this.form.reset({ switchNo: 'Non' });
    });
  }

  uninstall(id: number) {
    if (confirm('Uninstall this sensor?')) {
      this.deviceService.uninstallSensor(id).subscribe(() => {
        this.installed = this.installed.filter(s => s.id !== id);
      });
    }
  }
}
