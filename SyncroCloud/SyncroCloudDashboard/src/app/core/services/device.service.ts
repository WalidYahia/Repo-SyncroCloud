import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DeviceDto, CreateDeviceDto, DeviceSensorDto, CreateDeviceSensorDto, DeviceScenarioDto, UpsertDeviceScenarioDto } from '../models/device.models';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private url = `${environment.apiUrl}/devices`;
  constructor(private http: HttpClient) {}

  getAll()                           { return this.http.get<DeviceDto[]>(this.url); }
  getByTenant(tenantId: string)       { return this.http.get<DeviceDto[]>(`${this.url}/tenant/${tenantId}`); }
  getById(id: string)                 { return this.http.get<DeviceDto>(`${this.url}/${id}`); }
  create(dto: CreateDeviceDto)        { return this.http.post<DeviceDto>(this.url, dto); }
  delete(id: string)                  { return this.http.delete(`${this.url}/${id}`); }
  updateStatus(id: string, status: string) { return this.http.patch(`${this.url}/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }); }

  // Device Sensors
  getSensors(deviceId: string)        { return this.http.get<DeviceSensorDto[]>(`${environment.apiUrl}/devicesensors/device/${deviceId}`); }
  installSensor(dto: CreateDeviceSensorDto) { return this.http.post<DeviceSensorDto>(`${environment.apiUrl}/devicesensors`, dto); }
  uninstallSensor(id: number) { return this.http.delete(`${environment.apiUrl}/devicesensors/${id}`); }

  // Device Scenarios
  getScenarios(deviceId: string)      { return this.http.get<DeviceScenarioDto[]>(`${environment.apiUrl}/devicescenarios/device/${deviceId}`); }
  upsertScenario(scenarioId: string, dto: UpsertDeviceScenarioDto) { return this.http.put<DeviceScenarioDto>(`${environment.apiUrl}/devicescenarios/${scenarioId}`, dto); }
  deleteScenario(deviceId: string, scenarioId: string) { return this.http.delete(`${environment.apiUrl}/devicescenarios/device/${deviceId}/${scenarioId}`); }
}
