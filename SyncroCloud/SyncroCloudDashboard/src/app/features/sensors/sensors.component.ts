import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';
import { SensorService } from '../../core/services/sensor.service';
import { SensorDto } from '../../core/models/sensor.models';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule, MatCardModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './sensors.component.html',
  styleUrl: './sensors.component.scss'
})
export class SensorsComponent implements OnInit {
  private sensorService = inject(SensorService);
  private fb = inject(FormBuilder);

  sensors: SensorDto[] = [];
  columns = ['name', 'type', 'protocol', 'baseUrl', 'syncPeriodicity', 'eventSync', 'actions'];
  showForm = false;
  loading = signal(true);

  sensorTypes  = ['Unknown', 'SonOffMiniR3Swich', 'Temperature', 'Humidity', 'Pressure', 'Motion', 'Gas', 'Light', 'Vibration', 'Current', 'Voltage'];
  protocols    = ['MQTT', 'HTTP', 'CoAP', 'Modbus', 'Zigbee', 'ZWave', 'BLE', 'LoRa', 'RS485'];

  form = this.fb.group({
    name:               ['', Validators.required],
    type:               ['', Validators.required],
    connectionProtocol: ['', Validators.required],
    baseUrl:            [''],
    portNo:             [''],
    dataPath:           [''],
    infoPath:           [''],
    inchingPath:        [''],
    syncPeriodicity:    [null as number | null],
    eventChangeSync:    [false],
    eventChangeDelta:   [null as number | null]
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.sensorService.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: d => { this.sensors = d; }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.sensorService.create(this.form.value as any).subscribe(() => {
      this.form.reset({ eventChangeSync: false }); this.showForm = false; this.load();
    });
  }

  delete(id: string) {
    if (confirm('Delete this sensor?')) {
      this.sensorService.delete(id).subscribe(() => this.load());
    }
  }
}
