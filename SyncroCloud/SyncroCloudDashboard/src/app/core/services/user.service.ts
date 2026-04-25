import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserDto, CreateUserDto, UpdateUserDto } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private url = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) {}

  getAll()                           { return this.http.get<UserDto[]>(this.url); }
  getById(id: string)                { return this.http.get<UserDto>(`${this.url}/${id}`); }
  create(dto: CreateUserDto)         { return this.http.post<UserDto>(this.url, dto); }
  update(id: string, dto: UpdateUserDto) { return this.http.put<UserDto>(`${this.url}/${id}`, dto); }
  delete(id: string)                 { return this.http.delete(`${this.url}/${id}`); }
}
