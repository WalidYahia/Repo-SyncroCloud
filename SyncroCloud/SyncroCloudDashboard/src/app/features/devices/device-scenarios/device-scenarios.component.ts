import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DeviceService } from '../../../core/services/device.service';
import { DeviceScenarioDto } from '../../../core/models/device.models';

@Component({
  selector: 'app-device-scenarios',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, DatePipe, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatCardModule, MatTooltipModule],
  templateUrl: './device-scenarios.component.html',
  styleUrl: './device-scenarios.component.scss'
})
export class DeviceScenariosComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  private fb = inject(FormBuilder);

  deviceId!: string;
  scenarios: DeviceScenarioDto[] = [];
  columns = ['name', 'action', 'conditions', 'updatedAt', 'actions'];
  showForm = false;
  editingId: string | null = null;

  form = this.fb.group({
    payload: ['', [Validators.required]]
  });

  ngOnInit() {
    this.deviceId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    this.deviceService.getScenarios(this.deviceId).subscribe(s => this.scenarios = s);
  }

  parsedScenario(payload: string) {
    try { return JSON.parse(payload); } catch { return {}; }
  }

  startNew() {
    this.editingId = null;
    this.form.reset({ payload: JSON.stringify({ Id: crypto.randomUUID(), Name: '', IsEnabled: true, TargetSensorId: '', Action: 'On', LogicOfConditions: 'And', Conditions: [] }, null, 2) });
    this.showForm = true;
  }

  edit(scenario: DeviceScenarioDto) {
    this.editingId = scenario.id;
    this.form.setValue({ payload: JSON.stringify(JSON.parse(scenario.payload), null, 2) });
    this.showForm = true;
  }

  save() {
    if (this.form.invalid) return;
    const payload = this.form.value.payload!;
    const parsed = this.parsedScenario(payload);
    const scenarioId = this.editingId ?? parsed.Id ?? crypto.randomUUID();
    this.deviceService.upsertScenario(scenarioId, { deviceId: this.deviceId, payload }).subscribe(() => {
      this.showForm = false; this.editingId = null; this.load();
    });
  }

  delete(scenarioId: string) {
    if (confirm('Delete this scenario?')) {
      this.deviceService.deleteScenario(this.deviceId, scenarioId).subscribe(() => this.load());
    }
  }
}
