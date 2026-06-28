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
  sensorType: string;
  protocol: number;
  baseUrl: string;
  portNo: string;
  dataPath: string;
  infoPath: string;
  inchingPath: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
  onlySaveRecordOnChange: boolean;
  isInInchingMode: boolean;
  inchingModeWidthInMs: number;
  installedAt: string;
  isActive: boolean;
  notes: string | null;
  lastReading: string | null;
  lastSeen: string | null;
}

export interface UpdateDeviceSensorDto {
  switchNo: string;
  unitId: string;
  address: number | null;
  port: number | null;
  displayName: string;
  url: string | null;
  sensorType: string;
  protocol: number;
  dataPath: string;
  infoPath: string;
  inchingPath: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
  onlySaveRecordOnChange: boolean;
  isInInchingMode: boolean;
  inchingModeWidthInMs: number;
  isActive: boolean;
  notes: string | null;
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
  sensorType: string;
  protocol: number;
  baseUrl: string;
  portNo: string;
  dataPath: string;
  infoPath: string;
  inchingPath: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
  isInInchingMode: boolean;
  inchingModeWidthInMs: number;
  installedById: string | null;
}

export type ScenarioAction = 'Off' | 'On';
export type ScenarioConditionType = 'Duration' | 'OnTime' | 'OnOtherSensorValue';
export type ScenarioOperator = 'Equals' | 'NotEquals' | 'GreaterThan' | 'LessThan' | 'GreaterOrEqual' | 'LessOrEqual';
export type ScenarioLogic = 'And' | 'Or';

export interface UserScenarioSensor {
  sensorId: string;
  sensorType: number;
  value: string;
  operator: ScenarioOperator;
}

export interface UserScenarioCondition {
  condition: ScenarioConditionType;
  durationInSeconds: number;
  time: string | null;
  sensorsDependency: UserScenarioSensor[] | null;
}

export interface UserScenario {
  id: string;
  name: string;
  isEnabled: boolean;
  targetSensorId: string;
  action: ScenarioAction;
  logicOfConditions: ScenarioLogic;
  conditions: UserScenarioCondition[];
}
