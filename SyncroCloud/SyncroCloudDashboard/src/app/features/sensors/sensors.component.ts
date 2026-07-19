import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
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
import { AuthService } from '../../core/services/auth.service';
import { SensorDto } from '../../core/models/sensor.models';
import { sortRows } from '../../shared/utils/table-sort';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [ReactiveFormsModule, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule, MatCardModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './sensors.component.html',
  styleUrl: './sensors.component.scss'
})
export class SensorsComponent implements OnInit {
  private sensorService = inject(SensorService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  sensors = signal<SensorDto[]>([]);
  columns = ['name', 'type', 'protocol', 'baseUrl', 'syncPeriodicity', 'eventSync', 'actions'];
  showForm = false;
  loading = signal(true);

  sort = signal<Sort>({ active: '', direction: '' });
  sortedSensors = computed(() =>
    sortRows(this.sensors(), this.sort(), (s, col) => this.sortValue(s, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(s: SensorDto, col: string): unknown {
    switch (col) {
      case 'name':            return s.name;
      case 'type':            return s.type;
      case 'protocol':        return s.connectionProtocol;
      case 'baseUrl':         return s.baseUrl;
      case 'syncPeriodicity': return s.syncPeriodicity;
      case 'eventSync':       return s.eventChangeSync ? 1 : 0;
      default:                return '';
    }
  }

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
      next: d => { this.sensors.set(d); }
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
