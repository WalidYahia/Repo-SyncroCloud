import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TenantDto, CreateTenantDto, UpdateTenantDto } from '../models/tenant.models';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private url = `${environment.apiUrl}/tenants`;
  constructor(private http: HttpClient) {}

  getAll()                               { return this.http.get<TenantDto[]>(this.url); }
  getById(id: string)                    { return this.http.get<TenantDto>(`${this.url}/${id}`); }
  create(dto: CreateTenantDto)           { return this.http.post<TenantDto>(this.url, dto); }
  update(id: string, dto: UpdateTenantDto) { return this.http.put<TenantDto>(`${this.url}/${id}`, dto); }
  delete(id: string)                     { return this.http.delete(`${this.url}/${id}`); }
}
