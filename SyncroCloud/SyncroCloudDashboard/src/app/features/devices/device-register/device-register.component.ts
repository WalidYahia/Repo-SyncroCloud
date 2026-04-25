import { Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DeviceService } from '../../../core/services/device.service';
import { TenantService } from '../../../core/services/tenant.service';
import { LocationService } from '../../../core/services/location.service';
import { TenantDto } from '../../../core/models/tenant.models';
import { CountryDto, CityDto } from '../../../core/models/location.models';

@Component({
  selector: 'app-device-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  templateUrl: './device-register.component.html',
  styleUrl: './device-register.component.scss'
})
export class DeviceRegisterComponent implements OnInit {
  private fb              = inject(FormBuilder);
  private deviceService   = inject(DeviceService);
  private tenantService   = inject(TenantService);
  private locationService = inject(LocationService);
  private router          = inject(Router);

  tenants:     TenantDto[]  = [];
  countries:   CountryDto[] = [];
  cities:      CityDto[]    = [];
  deviceTypes = ['SmartHome', 'Monitoring'];

  form = this.fb.group({
    deviceId:     ['', Validators.required],
    name:         ['', Validators.required],
    serialNumber: ['', Validators.required],
    tenantId:     ['', Validators.required],
    type:         ['', Validators.required],
    countryId:    [null as number | null, Validators.required],
    cityId:       [null as number | null, Validators.required],
  });

  ngOnInit() {
    this.tenantService.getAll().subscribe(t => this.tenants = t);
    this.locationService.getCountries().subscribe(c => this.countries = c);
  }

  onCountryChange(countryId: number) {
    this.cities = [];
    this.form.patchValue({ cityId: null });
    if (countryId) {
      this.locationService.getCitiesByCountry(countryId)
        .subscribe(cities => this.cities = cities);
    }
  }

  save() {
    if (this.form.invalid) return;
    const { countryId, ...dto } = this.form.value;
    this.deviceService.create(dto as any).subscribe(device => {
      this.router.navigate(['/devices', device.id, 'sensors']);
    });
  }
}
