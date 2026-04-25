import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { TenantService } from '../../core/services/tenant.service';
import { TenantDto } from '../../core/models/tenant.models';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatChipsModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './tenants.component.html',
  styleUrl: './tenants.component.scss'
})
export class TenantsComponent implements OnInit {
  private tenantService = inject(TenantService);
  private fb = inject(FormBuilder);

  tenants: TenantDto[] = [];
  columns = ['name', 'status', 'createdAt', 'actions'];
  form = this.fb.group({ name: ['', Validators.required] });
  showForm = false;
  loading = signal(true);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.tenantService.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: d => { this.tenants = d; }
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
