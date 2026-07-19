import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { PrivilegeDto, RoleDetailDto } from '../../../core/models/role.models';
import { RoleService } from '../../../core/services/role.service';

export interface CreateRoleDialogData {
  privileges: PrivilegeDto[];
  role?: RoleDetailDto; // provided when editing privileges
}

@Component({
  selector: 'app-create-role-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, MatCheckboxModule, MatProgressSpinnerModule, DragDropModule],
  templateUrl: './create-role-dialog.component.html',
  styleUrl: './create-role-dialog.component.scss'
})
export class CreateRoleDialogComponent {
  private fb          = inject(FormBuilder);
  private dialogRef   = inject(MatDialogRef<CreateRoleDialogComponent>);
  private roleService = inject(RoleService);
  data = inject<CreateRoleDialogData>(MAT_DIALOG_DATA);

  isEditMode = !!this.data.role;
  privileges = this.data.privileges;

  selectedIds = signal<string[]>(
    this.data.role?.privileges.map(p => p.id) ?? []
  );
  isSaving    = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    name: [this.data.role?.name ?? '', Validators.required],
  });

  isSelected(id: string): boolean {
    return this.selectedIds().includes(id);
  }

  togglePrivilege(id: string, checked: boolean) {
    this.selectedIds.update(ids =>
      checked ? [...ids, id] : ids.filter(x => x !== id)
    );
  }

  save() {
    if (this.form.invalid || this.isSaving()) return;

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const privilegeIds = this.selectedIds();

    if (this.isEditMode) {
      this.roleService.updatePrivileges(this.data.role!.id, { privilegeIds }).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.dialogRef.close(true);
        },
        error: err => {
          this.isSaving.set(false);
          this.errorMessage.set(err.error?.message ?? 'An error occurred. Please try again.');
        }
      });
    } else {
      this.roleService.create({ name: this.form.value.name!, privilegeIds }).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.dialogRef.close(true);
        },
        error: err => {
          this.isSaving.set(false);
          this.errorMessage.set(err.error?.message ?? 'An error occurred. Please try again.');
        }
      });
    }
  }

  cancel() {
    this.dialogRef.close();
  }
}
