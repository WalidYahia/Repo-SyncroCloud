import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserDto, CreateUserDto, UpdateUserDto, UpdateUserRoleDto, RoleDto } from '../models/user.models';
import { TenantDto } from '../models/tenant.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private url = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) {}

  getAll()                            { return this.http.get<UserDto[]>(this.url); }
  getByTenant(tenantId: string)       { return this.http.get<UserDto[]>(`${this.url}/tenant/${tenantId}`); }
  getById(id: string)                 { return this.http.get<UserDto>(`${this.url}/${id}`); }
  getTenants(id: string)              { return this.http.get<TenantDto[]>(`${this.url}/${id}/tenants`); }
  getRoles()                          { return this.http.get<RoleDto[]>(`${this.url}/roles`); }
  create(dto: CreateUserDto)          { return this.http.post<UserDto>(this.url, dto); }
  update(id: string, dto: UpdateUserDto) { return this.http.put<UserDto>(`${this.url}/${id}`, dto); }
  updateRole(id: string, dto: UpdateUserRoleDto) { return this.http.patch<UserDto>(`${this.url}/${id}/role`, dto); }
  addToTenant(id: string, tenantId: string) { return this.http.post(`${this.url}/${id}/tenants/${tenantId}`, {}); }
  removeFromTenant(id: string, tenantId: string) { return this.http.delete(`${this.url}/${id}/tenants/${tenantId}`); }
  delete(id: string)                  { return this.http.delete(`${this.url}/${id}`); }
}
