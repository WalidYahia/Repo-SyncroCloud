import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DeviceDto, CreateDeviceDto, DeviceSensorDto, CreateDeviceSensorDto, UpdateDeviceSensorDto, UserScenario, DeviceUserDto, SensorPermissionDto } from '../models/device.models';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private url = `${environment.apiUrl}/devices`;
  constructor(private http: HttpClient) {}

  getAll()                           { return this.http.get<DeviceDto[]>(this.url); }
  getByTenant(tenantId: string)       { return this.http.get<DeviceDto[]>(`${this.url}/tenant/${tenantId}`); }
  getByUser(userId: string)           { return this.http.get<DeviceDto[]>(`${this.url}/user/${userId}`); }
  getById(id: string)                 { return this.http.get<DeviceDto>(`${this.url}/${id}`); }
  create(dto: CreateDeviceDto)        { return this.http.post<DeviceDto>(this.url, dto); }
  delete(id: string)                  { return this.http.delete(`${this.url}/${id}`); }
  updateStatus(id: string, status: string) { return this.http.patch(`${this.url}/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }); }

  // Device-User assignment & sensor permissions
  getDeviceUsers(deviceId: string)    { return this.http.get<DeviceUserDto[]>(`${this.url}/${deviceId}/users`); }
  getUserLink(deviceId: string, userId: string) { return this.http.get<DeviceUserDto>(`${this.url}/${deviceId}/users/${userId}`); }
  assignUser(deviceId: string, userId: string)  { return this.http.post<DeviceUserDto>(`${this.url}/${deviceId}/users/${userId}`, {}); }
  removeUser(deviceId: string, userId: string)  { return this.http.delete(`${this.url}/${deviceId}/users/${userId}`); }
  updateSensorPermissions(deviceId: string, userId: string, permissions: SensorPermissionDto[]) {
    return this.http.put<DeviceUserDto>(`${this.url}/${deviceId}/users/${userId}/permissions`, permissions);
  }

  // Device Sensors
  getSensors(deviceId: string)        { return this.http.get<DeviceSensorDto[]>(`${environment.apiUrl}/devicesensors/device/${deviceId}`); }
  installSensor(dto: CreateDeviceSensorDto) { return this.http.post<DeviceSensorDto>(`${environment.apiUrl}/devicesensors`, dto); }
  updateSensor(id: string, dto: UpdateDeviceSensorDto) { return this.http.put<DeviceSensorDto>(`${environment.apiUrl}/devicesensors/${id}`, dto); }
  uninstallSensor(id: string) { return this.http.delete(`${environment.apiUrl}/devicesensors/${id}`); }

  // Device Scenarios
  getScenarios(deviceId: string)      { return this.http.get<UserScenario[]>(`${environment.apiUrl}/devicescenarios/device/${deviceId}`); }
  createScenario(deviceId: string, scenario: UserScenario) { return this.http.post<UserScenario>(`${environment.apiUrl}/devicescenarios/device/${deviceId}`, scenario); }
  updateScenario(deviceId: string, scenarioId: string, scenario: UserScenario) { return this.http.put<UserScenario>(`${environment.apiUrl}/devicescenarios/device/${deviceId}/${scenarioId}`, scenario); }
  deleteScenario(deviceId: string, scenarioId: string) { return this.http.delete(`${environment.apiUrl}/devicescenarios/device/${deviceId}/${scenarioId}`); }
}
