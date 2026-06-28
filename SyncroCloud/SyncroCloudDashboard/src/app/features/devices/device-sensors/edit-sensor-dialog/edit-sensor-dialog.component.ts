import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DeviceSensorDto } from '../../../../core/models/device.models';

export interface EditSensorDialogData {
  sensor: DeviceSensorDto;
}

export interface EditSensorDialogResult {
  displayName: string;
  address: number | null;
  port: number | null;
}

@Component({
  selector: 'app-edit-sensor-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, DragDropModule],
  templateUrl: './edit-sensor-dialog.component.html',
  styleUrl: './edit-sensor-dialog.component.scss'
})
export class EditSensorDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<EditSensorDialogComponent>);
  data = inject<EditSensorDialogData>(MAT_DIALOG_DATA);

  form = this.fb.group({
    displayName: [this.data.sensor.displayName, Validators.required],
    address:     [this.data.sensor.address as number | null],
    port:        [this.data.sensor.port as number | null],
  });

  save() {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.getRawValue() as EditSensorDialogResult);
  }

  cancel() {
    this.dialogRef.close();
  }
}
