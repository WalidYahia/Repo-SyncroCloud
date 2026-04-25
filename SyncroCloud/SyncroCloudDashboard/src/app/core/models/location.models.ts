export interface CountryDto {
  id: number;
  name: string;
  isoCode: string;
}

export interface CityDto {
  id: number;
  countryId: number;
  name: string;
  latitude: number;
  longitude: number;
}

export interface SyncResultDto {
  countriesSynced: number;
  citiesSynced: number;
}
