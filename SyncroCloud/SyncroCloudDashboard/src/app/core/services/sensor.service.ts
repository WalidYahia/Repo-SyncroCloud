import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { SensorDto, CreateSensorDto, UpdateSensorDto } from '../models/sensor.models';

@Injectable({ providedIn: 'root' })
export class SensorService {
  private url = `${environment.apiUrl}/sensors`;
  constructor(private http: HttpClient) {}

  getAll()                                { return this.http.get<SensorDto[]>(this.url); }
  getById(id: string)                     { return this.http.get<SensorDto>(`${this.url}/${id}`); }
  create(dto: CreateSensorDto)            { return this.http.post<SensorDto>(this.url, dto); }
  update(id: string, dto: UpdateSensorDto) { return this.http.put<SensorDto>(`${this.url}/${id}`, dto); }
  delete(id: string)                      { return this.http.delete(`${this.url}/${id}`); }
}
