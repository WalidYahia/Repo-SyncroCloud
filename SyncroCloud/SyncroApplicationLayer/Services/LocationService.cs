using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class LocationService(SyncroDbContext db) : ILocationService
{
    public async Task<List<CountryDto>> GetAllCountriesAsync() =>
        await db.Countries.Select(c => ToCountryDto(c)).ToListAsync();

    public async Task<CountryDto?> GetCountryByIdAsync(int id)
    {
        var c = await db.Countries.FindAsync(id);
        return c is null ? null : ToCountryDto(c);
    }

    public async Task<CountryDto> CreateCountryAsync(CreateCountryDto dto)
    {
        var country = new Country { Name = dto.Name, IsoCode = dto.IsoCode };
        db.Countries.Add(country);
        await db.SaveChangesAsync();
        return ToCountryDto(country);
    }

    public async Task<bool> DeleteCountryAsync(int id)
    {
        var country = await db.Countries.FindAsync(id);
        if (country is null) return false;
        db.Countries.Remove(country);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<CityDto>> GetCitiesByCountryAsync(int countryId) =>
        await db.Cities.Where(c => c.CountryId == countryId).Select(c => ToCityDto(c)).ToListAsync();

    public async Task<CityDto?> GetCityByIdAsync(int id)
    {
        var c = await db.Cities.FindAsync(id);
        return c is null ? null : ToCityDto(c);
    }

    public async Task<CityDto> CreateCityAsync(CreateCityDto dto)
    {
        var city = new City { CountryId = dto.CountryId, Name = dto.Name, Latitude = dto.Latitude, Longitude = dto.Longitude };
        db.Cities.Add(city);
        await db.SaveChangesAsync();
        return ToCityDto(city);
    }

    public async Task<CityDto?> UpdateCityAsync(int id, UpdateCityDto dto)
    {
        var city = await db.Cities.FindAsync(id);
        if (city is null) return null;
        city.Name = dto.Name;
        city.Latitude = dto.Latitude;
        city.Longitude = dto.Longitude;
        await db.SaveChangesAsync();
        return ToCityDto(city);
    }

    public async Task<bool> DeleteCityAsync(int id)
    {
        var city = await db.Cities.FindAsync(id);
        if (city is null) return false;
        db.Cities.Remove(city);
        await db.SaveChangesAsync();
        return true;
    }

    private static CountryDto ToCountryDto(Country c) => new(c.Id, c.Name, c.IsoCode);
    private static CityDto ToCityDto(City c) => new(c.Id, c.CountryId, c.Name, c.Latitude, c.Longitude);
}
