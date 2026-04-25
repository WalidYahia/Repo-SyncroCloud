namespace SyncroApplicationLayer.DTOs;

public record CountryDto(int Id, string Name, string IsoCode);

public record CreateCountryDto(string Name, string IsoCode);

public record CityDto(int Id, int CountryId, string Name, double Latitude, double Longitude);

public record CreateCityDto(int CountryId, string Name, double Latitude, double Longitude);

public record UpdateCityDto(string Name, double Latitude, double Longitude);

public record SyncResultDto(int CountriesSynced, int CitiesSynced);
