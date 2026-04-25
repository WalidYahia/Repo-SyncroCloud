export interface SensorDto {
  sensorId: string;
  name: string;
  unitType: string;
  type: string;
  connectionProtocol: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
}

export interface CreateSensorDto {
  name: string;
  unitType: string;
  type: string;
  connectionProtocol: string;
  syncPeriodicity: number | null;
  eventChangeSync: boolean;
  eventChangeDelta: number | null;
}

export type UpdateSensorDto = CreateSensorDto;
