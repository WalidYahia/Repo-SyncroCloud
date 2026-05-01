export interface SensorDto {
  sensorId: string;
  name: string;
  type: string;
  connectionProtocol: string;
  baseUrl: string;
  portNo: string;
  dataPath: string;
  infoPath: string;
  inchingPath: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
}

export interface CreateSensorDto {
  name: string;
  type: string;
  connectionProtocol: string;
  baseUrl: string;
  portNo: string;
  dataPath: string;
  infoPath: string;
  inchingPath: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
}

export type UpdateSensorDto = CreateSensorDto;
