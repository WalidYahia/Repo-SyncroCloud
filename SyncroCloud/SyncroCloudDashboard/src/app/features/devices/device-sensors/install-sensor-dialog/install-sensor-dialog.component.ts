import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SensorDto } from '../../../../core/models/sensor.models';

export interface InstallSensorDialogData {
  availableSensors: SensorDto[];
}

export interface InstallSensorDialogResult {
  sensorId: string;
  switchNo: string;
  unitId: string;
  displayName: string;
  address: number | null;
  port: number | null;
  url: string;
  selectedSensor: SensorDto;
}

@Component({
  selector: 'app-install-sensor-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, MatSelectModule, MatChipsModule, DragDropModule],
  templateUrl: './install-sensor-dialog.component.html',
  styleUrl: './install-sensor-dialog.component.scss'
})
export class InstallSensorDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<InstallSensorDialogComponent>);
  data = inject<InstallSensorDialogData>(MAT_DIALOG_DATA);

  availableSensors = this.data.availableSensors;
  selectedSensor: SensorDto | null = null;

  switchNos = ['Non', 'Switch1', 'Switch2', 'Switch3', 'Switch4', 'Switch5', 'Switch6', 'Switch7', 'Switch8'];

  form = this.fb.group({
    sensorId:    ['', Validators.required],
    switchNo:    ['Non'],
    unitId:      ['', Validators.required],
    displayName: ['', Validators.required],
    address:     [null as number | null],
    port:        [null as number | null],
  });

  get computedUrl(): string {
    if (!this.selectedSensor) return '';
    return `${this.selectedSensor.baseUrl}${this.form.value.unitId ?? ''}:${this.selectedSensor.portNo}`;
  }

  onSensorChange(sensorId: string) {
    this.selectedSensor = this.availableSensors.find(s => s.sensorId === sensorId) ?? null;
    if (this.selectedSensor) {
      this.form.patchValue({ displayName: this.selectedSensor.name });
    }
  }

  save() {
    if (this.form.invalid || !this.selectedSensor) return;

    const result: InstallSensorDialogResult = {
      ...this.form.getRawValue(),
      url: this.computedUrl,
      selectedSensor: this.selectedSensor
    } as InstallSensorDialogResult;

    this.dialogRef.close(result);
  }

  cancel() {
    this.dialogRef.close();
  }
}
