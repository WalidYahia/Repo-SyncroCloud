import { Component, OnInit, computed, inject, signal, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { DeviceService } from '../../../core/services/device.service';
import { AuthService } from '../../../core/services/auth.service';
import { DeviceSensorDto, UserScenario } from '../../../core/models/device.models';
import { sortRows } from '../../../shared/utils/table-sort';
import { ScenarioDialogComponent, ScenarioDialogData } from './scenario-dialog/scenario-dialog.component';

@Component({
  selector: 'app-device-scenarios',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule,
            MatTableModule, MatSortModule, MatChipsModule, MatDialogModule],
  templateUrl: './device-scenarios.component.html',
  styleUrl: './device-scenarios.component.scss'
})
export class DeviceScenariosComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  auth = inject(AuthService);
  private deviceService = inject(DeviceService);
  private dialog         = inject(MatDialog);
  private cdr           = inject(ChangeDetectorRef);

  deviceId!: string;
  scenarios = signal<UserScenario[]>([]);
  sensors: DeviceSensorDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  columns = ['name', 'targetSensor', 'action', 'logic', 'conditions', 'status', 'actions'];

  sort = signal<Sort>({ active: '', direction: '' });
  sortedScenarios = computed(() =>
    sortRows(this.scenarios(), this.sort(), (s, col) => this.sortValue(s, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(s: UserScenario, col: string): unknown {
    switch (col) {
      case 'name':         return s.name;
      case 'targetSensor': return this.sensorName(s.targetSensorId);
      case 'action':       return s.action;
      case 'logic':        return s.logicOfConditions;
      case 'conditions':   return s.conditions.length;
      case 'status':       return s.isEnabled ? 1 : 0;
      default:             return '';
    }
  }

  ngOnInit() {
    this.deviceId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    this.isLoading = true;
    this.errorMessage = null;
    forkJoin([
      this.deviceService.getScenarios(this.deviceId),
      this.deviceService.getSensors(this.deviceId)
    ]).subscribe({
      next: ([scenarios, sensors]) => {
        this.scenarios.set(scenarios);
        this.sensors = sensors;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load scenarios.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  sensorName(sensorId: string): string {
    return this.sensors.find(s => s.id === sensorId)?.displayName ?? sensorId;
  }

  openDialog(scenario?: UserScenario) {
    const ref = this.dialog.open<ScenarioDialogComponent, ScenarioDialogData, UserScenario>(ScenarioDialogComponent, {
      width: '720px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      disableClose: true,
      data: { scenario: scenario ?? null, sensors: this.sensors }
    });

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      const request = scenario
        ? this.deviceService.updateScenario(this.deviceId, scenario.id, result)
        : this.deviceService.createScenario(this.deviceId, result);
      request.subscribe(() => this.load());
    });
  }

  delete(scenario: UserScenario) {
    if (confirm(`Delete scenario "${scenario.name}"?`)) {
      this.deviceService.deleteScenario(this.deviceId, scenario.id).subscribe(() => this.load());
    }
  }
}
