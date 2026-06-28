import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormArray, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DeviceSensorDto, UserScenario, UserScenarioCondition, UserScenarioSensor } from '../../../../core/models/device.models';

export interface ScenarioDialogData {
  scenario: UserScenario | null;
  sensors: DeviceSensorDto[];
}

@Component({
  selector: 'app-scenario-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, MatSelectModule, MatButtonToggleModule, MatSlideToggleModule,
            MatExpansionModule, MatDividerModule, MatTooltipModule, DragDropModule],
  templateUrl: './scenario-dialog.component.html',
  styleUrl: './scenario-dialog.component.scss'
})
export class ScenarioDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<ScenarioDialogComponent>);
  data = inject<ScenarioDialogData>(MAT_DIALOG_DATA);

  sensors = this.data.sensors;
  isEdit = !!this.data.scenario;

  readonly actions = ['Off', 'On'];
  readonly logics = ['And', 'Or'];
  readonly conditionTypes = ['Duration', 'OnTime', 'OnOtherSensorValue'];
  readonly operators = ['Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'GreaterOrEqual', 'LessOrEqual'];

  form = this.fb.group({
    name:              [this.data.scenario?.name ?? '', Validators.required],
    isEnabled:         [this.data.scenario?.isEnabled ?? true],
    targetSensorId:    [this.data.scenario?.targetSensorId ?? '', Validators.required],
    action:            [this.data.scenario?.action ?? 'On', Validators.required],
    logicOfConditions: [this.data.scenario?.logicOfConditions ?? 'And', Validators.required],
    conditions:        this.fb.array<FormGroup>((this.data.scenario?.conditions ?? []).map(c => this.newCondition(c)))
  });

  get conditions(): FormArray<FormGroup> {
    return this.form.get('conditions') as FormArray<FormGroup>;
  }

  dependencies(conditionIndex: number): FormArray<FormGroup> {
    return this.conditions.at(conditionIndex).get('sensorsDependency') as FormArray<FormGroup>;
  }

  private newDependency(d?: UserScenarioSensor): FormGroup {
    return this.fb.group({
      sensorId:   [d?.sensorId ?? '', Validators.required],
      sensorType: [d?.sensorType ?? 0],
      value:      [d?.value ?? '', Validators.required],
      operator:   [d?.operator ?? 'Equals', Validators.required]
    });
  }

  private newCondition(c?: UserScenarioCondition): FormGroup {
    return this.fb.group({
      condition:          [c?.condition ?? 'Duration', Validators.required],
      durationInSeconds:  [c?.durationInSeconds ?? 0],
      time:               [c?.time ?? ''],
      sensorsDependency:  this.fb.array((c?.sensorsDependency ?? []).map(d => this.newDependency(d)))
    });
  }

  addCondition() {
    this.conditions.push(this.newCondition());
  }

  removeCondition(index: number) {
    this.conditions.removeAt(index);
  }

  addDependency(conditionIndex: number) {
    this.dependencies(conditionIndex).push(this.newDependency());
  }

  removeDependency(conditionIndex: number, depIndex: number) {
    this.dependencies(conditionIndex).removeAt(depIndex);
  }

  save() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    const scenario: UserScenario = {
      id:                this.data.scenario?.id ?? '',
      name:              v.name!,
      isEnabled:         !!v.isEnabled,
      targetSensorId:    v.targetSensorId!,
      action:            v.action as UserScenario['action'],
      logicOfConditions: v.logicOfConditions as UserScenario['logicOfConditions'],
      conditions: (v.conditions ?? []).map((c: any) => ({
        condition:         c.condition,
        durationInSeconds: c.durationInSeconds ?? 0,
        time:              c.condition === 'OnTime' ? c.time : null,
        sensorsDependency: c.condition === 'OnOtherSensorValue' ? c.sensorsDependency : null
      }))
    };

    this.dialogRef.close(scenario);
  }

  cancel() {
    this.dialogRef.close();
  }
}
