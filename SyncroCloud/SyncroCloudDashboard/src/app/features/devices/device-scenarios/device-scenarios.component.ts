import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { DeviceService } from '../../../core/services/device.service';
import { DeviceSensorDto, UserScenario } from '../../../core/models/device.models';
import { ScenarioDialogComponent, ScenarioDialogData } from './scenario-dialog/scenario-dialog.component';

@Component({
  selector: 'app-device-scenarios',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule,
            MatTableModule, MatChipsModule, MatDialogModule],
  templateUrl: './device-scenarios.component.html',
  styleUrl: './device-scenarios.component.scss'
})
export class DeviceScenariosComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  private dialog         = inject(MatDialog);
  private cdr           = inject(ChangeDetectorRef);

  deviceId!: string;
  scenarios: UserScenario[] = [];
  sensors: DeviceSensorDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  columns = ['name', 'targetSensor', 'action', 'logic', 'conditions', 'status', 'actions'];

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
        this.scenarios = scenarios;
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
