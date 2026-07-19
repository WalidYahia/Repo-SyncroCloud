import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { PrivilegeDto, RoleDetailDto, CreateRoleDto, UpdateRolePrivilegesDto } from '../models/role.models';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private url = `${environment.apiUrl}/roles`;
  constructor(private http: HttpClient) {}

  getAll()                              { return this.http.get<RoleDetailDto[]>(this.url); }
  getById(id: string)                   { return this.http.get<RoleDetailDto>(`${this.url}/${id}`); }
  getPrivileges()                       { return this.http.get<PrivilegeDto[]>(`${this.url}/privileges`); }
  create(dto: CreateRoleDto)            { return this.http.post<RoleDetailDto>(this.url, dto); }
  updatePrivileges(id: string, dto: UpdateRolePrivilegesDto) { return this.http.put(`${this.url}/${id}/privileges`, dto); }
  delete(id: string)                    { return this.http.delete(`${this.url}/${id}`); }
}
