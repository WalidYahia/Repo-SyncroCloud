import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { RoleDto, UserDto, UpdateUserDto } from '../../../core/models/user.models';

export interface EditUserDialogData {
  user: UserDto;
  roles: RoleDto[];
}

export interface EditUserDialogResult {
  update: UpdateUserDto;
  roleId: string | null;
}

@Component({
  selector: 'app-edit-user-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, MatSelectModule, MatSlideToggleModule, MatTooltipModule, DragDropModule],
  templateUrl: './edit-user-dialog.component.html',
  styleUrl: './edit-user-dialog.component.scss'
})
export class EditUserDialogComponent {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<EditUserDialogComponent>);
  data = inject<EditUserDialogData>(MAT_DIALOG_DATA);

  roles = this.data.roles;

  private currentRoleId = this.roles.find(r => r.name === this.data.user.roles[0])?.id ?? '';

  form = this.fb.group({
    email:     [this.data.user.email ?? ''],
    firstName: [this.data.user.firstName, Validators.required],
    lastName:  [this.data.user.lastName, Validators.required],
    isActive:  [this.data.user.isActive],
    roleId:    [this.currentRoleId],
  });

  save() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    const result: EditUserDialogResult = {
      update: {
        email:     v.email || null,
        firstName: v.firstName!,
        lastName:  v.lastName!,
        isActive:  !!v.isActive,
      },
      roleId: v.roleId && v.roleId !== this.currentRoleId ? v.roleId : null,
    };

    this.dialogRef.close(result);
  }

  cancel() {
    this.dialogRef.close();
  }
}
