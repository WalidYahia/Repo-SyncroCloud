import { Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LocationService } from '../../core/services/location.service';
import { SyncResultDto } from '../../core/models/location.models';

@Component({
  selector: 'app-geo-sync',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './geo-sync.component.html',
  styleUrl: './geo-sync.component.scss'
})
export class GeoSyncComponent {
  private locationService = inject(LocationService);

  syncing = signal(false);
  result  = signal<SyncResultDto | null>(null);

  sync() {
    this.syncing.set(true);
    this.result.set(null);
    this.locationService.sync().subscribe({
      next: r  => { this.result.set(r); this.syncing.set(false); },
      error: () => this.syncing.set(false)
    });
  }
}
