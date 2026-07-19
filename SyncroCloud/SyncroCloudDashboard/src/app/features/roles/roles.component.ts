import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { RoleService } from '../../core/services/role.service';
import { AuthService } from '../../core/services/auth.service';
import { RoleDetailDto, PrivilegeDto } from '../../core/models/role.models';
import { sortRows } from '../../shared/utils/table-sort';
import { CreateRoleDialogComponent, CreateRoleDialogData } from './create-role-dialog/create-role-dialog.component';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatChipsModule,
            MatTooltipModule, MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit {
  private roleService = inject(RoleService);
  auth = inject(AuthService);
  private dialog       = inject(MatDialog);

  roles = signal<RoleDetailDto[]>([]);
  allPrivileges: PrivilegeDto[] = [];
  columns = ['name', 'privileges', 'actions'];
  loading = signal(true);

  readonly builtIn = ['SuperAdmin', 'TenantAdmin', 'User'];

  sort = signal<Sort>({ active: '', direction: '' });
  sortedRoles = computed(() =>
    sortRows(this.roles(), this.sort(), (r, col) => this.sortValue(r, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(r: RoleDetailDto, col: string): unknown {
    switch (col) {
      case 'name':       return r.name;
      case 'privileges': return r.privileges.length;
      default:           return '';
    }
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    forkJoin([
      this.roleService.getAll(),
      this.roleService.getPrivileges()
    ]).subscribe({
      next: ([roles, privileges]) => {
        this.roles.set(roles);
        this.allPrivileges = privileges;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreateDialog() {
    const ref = this.dialog.open<CreateRoleDialogComponent, CreateRoleDialogData, true>(
      CreateRoleDialogComponent,
      {
        width: '520px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { privileges: this.allPrivileges }
      }
    );
    ref.afterClosed().subscribe(success => { if (success) this.load(); });
  }

  openEditPrivilegesDialog(role: RoleDetailDto) {
    const ref = this.dialog.open<CreateRoleDialogComponent, CreateRoleDialogData, true>(
      CreateRoleDialogComponent,
      {
        width: '520px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { privileges: this.allPrivileges, role }
      }
    );
    ref.afterClosed().subscribe(success => { if (success) this.load(); });
  }

  delete(role: RoleDetailDto) {
    if (confirm(`Delete role "${role.name}"? This cannot be undone.`)) {
      this.roleService.delete(role.id).subscribe(() => this.load());
    }
  }

  isBuiltIn(name: string): boolean {
    return this.builtIn.includes(name);
  }
}
