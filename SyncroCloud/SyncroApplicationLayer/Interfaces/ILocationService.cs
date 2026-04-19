using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface ILocationService
{
    Task<List<CountryDto>> GetAllCountriesAsync();
    Task<CountryDto?> GetCountryByIdAsync(int id);
    Task<CountryDto> CreateCountryAsync(CreateCountryDto dto);
    Task<bool> DeleteCountryAsync(int id);

    Task<List<CityDto>> GetCitiesByCountryAsync(int countryId);
    Task<CityDto?> GetCityByIdAsync(int id);
    Task<CityDto> CreateCityAsync(CreateCityDto dto);
    Task<CityDto?> UpdateCityAsync(int id, UpdateCityDto dto);
    Task<bool> DeleteCityAsync(int id);
}
