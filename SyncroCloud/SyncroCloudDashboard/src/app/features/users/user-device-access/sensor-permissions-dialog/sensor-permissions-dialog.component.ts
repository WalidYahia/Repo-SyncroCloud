import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormArray, FormGroup } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DeviceDto, DeviceSensorDto, SensorPermissionDto, SensorAccessLevel } from '../../../../core/models/device.models';

export interface SensorPermissionsDialogData {
  device: DeviceDto;
  sensors: DeviceSensorDto[];
  existingPermissions: SensorPermissionDto[];
}

@Component({
  selector: 'app-sensor-permissions-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatSelectModule, DragDropModule],
  templateUrl: './sensor-permissions-dialog.component.html',
  styleUrl: './sensor-permissions-dialog.component.scss'
})
export class SensorPermissionsDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<SensorPermissionsDialogComponent>);
  data = inject<SensorPermissionsDialogData>(MAT_DIALOG_DATA);

  readonly accessOptions: SensorAccessLevel[] = ['Watch', 'Control'];

  rows: FormArray<FormGroup> = this.fb.array(this.data.sensors.map(s => {
    const existing = this.data.existingPermissions.find(p => p.sensorId === s.id);
    return this.fb.group({
      sensorId:    [s.id],
      displayName: [s.displayName],
      access:      [(existing?.access ?? 'Watch') as SensorAccessLevel]
    });
  }));

  save() {
    const permissions: SensorPermissionDto[] = this.rows.controls
      .map(c => c.value)
      .map(r => ({ sensorId: r.sensorId, access: r.access as SensorAccessLevel }));

    this.dialogRef.close(permissions);
  }

  cancel() {
    this.dialogRef.close();
  }
}
