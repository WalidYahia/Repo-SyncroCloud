import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { TenantDto } from '../../../core/models/tenant.models';
import { RoleDto, CreateUserDto } from '../../../core/models/user.models';
import { UserService } from '../../../core/services/user.service';

export interface CreateUserDialogData {
  creatorTenants: TenantDto[];
  roles: RoleDto[];
}

@Component({
  selector: 'app-create-user-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule,
            MatInputModule, MatSelectModule, MatCheckboxModule, MatProgressSpinnerModule, DragDropModule],
  templateUrl: './create-user-dialog.component.html',
  styleUrl: './create-user-dialog.component.scss'
})
export class CreateUserDialogComponent {
  private fb          = inject(FormBuilder);
  private dialogRef   = inject(MatDialogRef<CreateUserDialogComponent>);
  private userService = inject(UserService);
  data = inject<CreateUserDialogData>(MAT_DIALOG_DATA);

  creatorTenants = this.data.creatorTenants;
  roles          = this.data.roles;

  isSaving = signal(false);
  errorMessage = signal<string | null>(null);

  get isSuperAdmin(): boolean {
    return this.roles.find(r => r.id === this.form.value.roleId)?.name === 'SuperAdmin';
  }

  get isMultiTenant(): boolean {
    return this.creatorTenants.length > 1;
  }

  form = this.fb.group({
    phoneNumber: ['', Validators.required],
    password:    ['', [Validators.required, Validators.minLength(8)]],
    firstName:   ['', Validators.required],
    lastName:    ['', Validators.required],
    email:       [''],
    roleId:      ['', Validators.required],
    tenantIds:   [[] as string[]],
  });

  toggleTenant(tenantId: string, checked: boolean) {
    const current = this.form.value.tenantIds as string[];
    const updated = checked
      ? [...current, tenantId]
      : current.filter(id => id !== tenantId);
    this.form.patchValue({ tenantIds: updated });
  }

  isTenantSelected(tenantId: string): boolean {
    return (this.form.value.tenantIds as string[]).includes(tenantId);
  }

  get isFormValid(): boolean {
    if (this.form.invalid) return false;
    if (this.isSuperAdmin) return true;
    if (!this.isMultiTenant) return true;
    return (this.form.value.tenantIds as string[]).length > 0;
  }

  save() {
    if (!this.isFormValid || this.isSaving()) return;

    const v = this.form.getRawValue();

    let tenantIds: string[];
    if (this.isSuperAdmin) {
      tenantIds = [];
    } else if (this.isMultiTenant) {
      tenantIds = v.tenantIds as string[];
    } else {
      tenantIds = this.creatorTenants.map(t => t.id);
    }

    const dto: CreateUserDto = {
      phoneNumber: v.phoneNumber!,
      password:    v.password!,
      firstName:   v.firstName!,
      lastName:    v.lastName!,
      roleId:      v.roleId!,
      tenantIds,
      email:       v.email || null,
    };

    this.isSaving.set(true);
    this.errorMessage.set(null);

    this.userService.create(dto).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isSaving.set(false);
        this.errorMessage.set(
          err.error?.message ?? err.error?.detail ?? 'An error occurred. Please try again.'
        );
      }
    });
  }

  cancel() {
    this.dialogRef.close();
  }
}
