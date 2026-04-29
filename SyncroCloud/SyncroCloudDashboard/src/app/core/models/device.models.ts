export interface DeviceDto {
  deviceId: string;
  tenantId: string;
  cityId: number;
  name: string;
  serialNumber: string;
  longitude: number | null;
  latitude: number | null;
  type: string;
  status: string;
  registeredAt: string;
  lastSeenAt: string | null;
}

export interface CreateDeviceDto {
  deviceId: string;
  tenantId: string;
  cityId: number;
  name: string;
  serialNumber: string;
  type: string;
}

export interface DeviceSensorDto {
  id: string;
  deviceId: string;
  sensorId: string;
  switchNo: string;
  unitId: string;
  address: number | null;
  port: number | null;
  displayName: string;
  url: string;
  unitType: string;
  sensorType: string;
  protocol: number;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
  installedAt: string;
  isActive: boolean;
  notes: string | null;
  lastReading: string | null;
}

export interface CreateDeviceSensorDto {
  deviceId: string;
  sensorId: string;
  switchNo: string;
  unitId: string;
  address: number | null;
  port: number | null;
  displayName: string;
  url: string;
  unitType: string;
  sensorType: string;
  protocol: number;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
  installedById: string | null;
}

export interface DeviceScenarioDto {
  id: string;
  deviceId: string;
  payload: string;
  updatedAt: string;
}

export interface UpsertDeviceScenarioDto {
  deviceId: string;
  payload: string;
}
