import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CountryDto, CityDto, SyncResultDto } from '../models/location.models';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(HttpClient);
  private url  = `${environment.apiUrl}/location`;

  getCountries()                        { return this.http.get<CountryDto[]>(`${this.url}/countries`); }
  getCitiesByCountry(countryId: number) { return this.http.get<CityDto[]>(`${this.url}/countries/${countryId}/cities`); }

  sync() {
    return this.http.post<SyncResultDto>(`${this.url}/sync`, {}, {
      headers: { 'X-Long-Timeout': 'true' }
    });
  }
}
