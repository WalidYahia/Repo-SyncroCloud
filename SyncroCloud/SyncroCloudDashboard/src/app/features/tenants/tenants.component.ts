import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { TenantService } from '../../core/services/tenant.service';
import { AuthService } from '../../core/services/auth.service';
import { TenantDto } from '../../core/models/tenant.models';
import { sortRows } from '../../shared/utils/table-sort';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatChipsModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './tenants.component.html',
  styleUrl: './tenants.component.scss'
})
export class TenantsComponent implements OnInit {
  private tenantService = inject(TenantService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  tenants = signal<TenantDto[]>([]);
  columns = ['name', 'status', 'createdAt', 'actions'];
  form = this.fb.group({ name: ['', Validators.required] });
  showForm = false;
  loading = signal(true);

  sort = signal<Sort>({ active: '', direction: '' });
  sortedTenants = computed(() =>
    sortRows(this.tenants(), this.sort(), (t, col) => this.sortValue(t, col))
  );
  onSort(s: Sort) { this.sort.set(s); }

  private sortValue(t: TenantDto, col: string): unknown {
    switch (col) {
      case 'name':      return t.name;
      case 'status':    return t.isActive ? 1 : 0;
      case 'createdAt': return new Date(t.createdAt).getTime();
      default:          return '';
    }
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.tenantService.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: d => { this.tenants.set(d); }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.tenantService.create(this.form.value as any).subscribe(() => {
      this.form.reset(); this.showForm = false; this.load();
    });
  }

  delete(id: string) {
    if (confirm('Delete this tenant?')) {
      this.tenantService.delete(id).subscribe(() => this.load());
    }
  }
}
