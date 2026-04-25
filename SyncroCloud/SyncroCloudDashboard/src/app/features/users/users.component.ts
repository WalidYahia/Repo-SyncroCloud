import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { UserService } from '../../core/services/user.service';
import { UserDto } from '../../core/models/user.models';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DatePipe, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private userService = inject(UserService);

  users: UserDto[] = [];
  columns = ['name', 'email', 'status', 'createdAt', 'actions'];
  loading = signal(true);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.userService.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: data => { this.users = data; }
    });
  }

  delete(id: string) {
    if (confirm('Delete this user?')) {
      this.userService.delete(id).subscribe(() => this.load());
    }
  }
}
