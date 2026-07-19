import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin, of, switchMap } from 'rxjs';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { PrivilegeCodes } from '../../core/identity/privilege-codes';
import { UserDto, RoleDto } from '../../core/models/user.models';
import { TenantDto } from '../../core/models/tenant.models';
import { sortRows } from '../../shared/utils/table-sort';
import { CreateUserDialogComponent, CreateUserDialogData } from './create-user-dialog/create-user-dialog.component';
import { EditUserDialogComponent, EditUserDialogData, EditUserDialogResult } from './edit-user-dialog/edit-user-dialog.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [RouterLink, DatePipe, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatChipsModule,
            MatTooltipModule, MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private userService = inject(UserService);
  auth = inject(AuthService);
  readonly P = PrivilegeCodes;
  private dialog       = inject(MatDialog);

  users = signal<UserDto[]>([]);
  creatorTenants: TenantDto[] = [];
  roles: RoleDto[] = [];
  columns = ['name', 'phone', 'email', 'roles', 'status', 'createdAt', 'actions'];
  loading = signal(true);

  sort = signal<Sort>({ active: '', direction: '' });
  sortedUsers = computed(() =>
    sortRows(this.users(), this.sort(), (u, col) => this.sortValue(u, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(u: UserDto, col: string): unknown {
    switch (col) {
      case 'name':      return `${u.firstName} ${u.lastName}`;
      case 'phone':     return u.phoneNumber;
      case 'email':     return u.email;
      case 'roles':     return u.roles.join(', ');
      case 'status':    return u.isActive ? 1 : 0;
      case 'createdAt': return new Date(u.createdAt).getTime();
      default:          return '';
    }
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    const creatorId = this.auth.getCurrentUserId();

    forkJoin([
      this.userService.getAll(),
      this.userService.getRoles()
    ]).pipe(
      switchMap(([users, roles]) => {
        this.users.set(users);
        this.roles = roles;
        // Load only the creator's own tenants for the "New User" dialog
        return creatorId ? this.userService.getTenants(creatorId) : of([]);
      })
    ).subscribe({
      next: tenants => {
        this.creatorTenants = tenants;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreateDialog() {
    const ref = this.dialog.open<CreateUserDialogComponent, CreateUserDialogData, true>(
      CreateUserDialogComponent,
      {
        width: '560px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { creatorTenants: this.creatorTenants, roles: this.roles }
      }
    );

    ref.afterClosed().subscribe(success => {
      if (success) this.load();
    });
  }

  openEditDialog(user: UserDto) {
    const ref = this.dialog.open<EditUserDialogComponent, EditUserDialogData, EditUserDialogResult>(
      EditUserDialogComponent,
      {
        width: '480px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        disableClose: true,
        data: { user, roles: this.roles }
      }
    );

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.userService.update(user.id, result.update).subscribe(() => {
        if (result.roleId) {
          this.userService.updateRole(user.id, { roleId: result.roleId }).subscribe(() => this.load());
        } else {
          this.load();
        }
      });
    });
  }

  delete(id: string) {
    if (confirm('Delete this user?')) {
      this.userService.delete(id).subscribe(() => this.load());
    }
  }
}
