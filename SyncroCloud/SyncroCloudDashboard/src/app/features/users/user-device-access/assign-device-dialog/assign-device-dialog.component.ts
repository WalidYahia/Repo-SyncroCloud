import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DeviceDto } from '../../../../core/models/device.models';

export interface AssignDeviceDialogData {
  availableDevices: DeviceDto[];
}

@Component({
  selector: 'app-assign-device-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatSelectModule, DragDropModule],
  templateUrl: './assign-device-dialog.component.html',
  styleUrl: './assign-device-dialog.component.scss'
})
export class AssignDeviceDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<AssignDeviceDialogComponent>);
  data = inject<AssignDeviceDialogData>(MAT_DIALOG_DATA);

  devices = this.data.availableDevices;

  form = this.fb.group({
    deviceId: ['', Validators.required],
  });

  save() {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.value.deviceId!);
  }

  cancel() {
    this.dialogRef.close();
  }
}
