using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class LocationSyncService(SyncroDbContext db, HttpClient httpClient) : ILocationSyncService
{
    private const string CountriesUrl  = "https://restcountries.com/v3.1/all?fields=name,cca2";
    private const string CitiesDataset =
        "https://public.opendatasoft.com/api/explore/v2.1/catalog/datasets/" +
        "geonames-all-cities-with-a-population-1000/records";

    public async Task<SyncResultDto> SyncAsync()
    {
        var countriesSynced = await SyncCountriesAsync();
        var citiesSynced    = await SyncCitiesAsync();
        return new SyncResultDto(countriesSynced, citiesSynced);
    }

    private async Task<int> SyncCountriesAsync()
    {
        var countries = await httpClient.GetFromJsonAsync<List<RestCountry>>(CountriesUrl) ?? [];
        var existing  = (await db.Countries.Select(c => c.IsoCode).ToListAsync()).ToHashSet();

        var toAdd = countries
            .Where(c => !string.IsNullOrEmpty(c.Cca2) && !existing.Contains(c.Cca2))
            .Select(c => new Country { Name = c.Name.Common, IsoCode = c.Cca2 })
            .ToList();

        db.Countries.AddRange(toAdd);
        await db.SaveChangesAsync();
        return toAdd.Count;
    }

    private async Task<int> SyncCitiesAsync()
    {
        var first      = await FetchCityPageAsync(0);
        var allRecords = new List<GeoCityRecord>(first.Results);

        var remainingPages = Enumerable.Range(1, first.TotalCount / 100)
            .Select(i => FetchCityPageAsync(i * 100));

        foreach (var batch in remainingPages.Chunk(10))
        {
            var pages = await Task.WhenAll(batch);
            foreach (var page in pages)
                allRecords.AddRange(page.Results);
        }

        var countryLookup  = await db.Countries.ToDictionaryAsync(c => c.IsoCode, c => c.Id);
        var existingCities = (await db.Cities.Select(c => new { c.CountryId, c.Name }).ToListAsync())
            .Select(c => $"{c.CountryId}:{c.Name}").ToHashSet();

        var toAdd = new List<City>();
        foreach (var rec in allRecords.DistinctBy(r => $"{r.CountryCode}:{r.Name}"))
        {
            if (string.IsNullOrEmpty(rec.CountryCode)) continue;
            if (!countryLookup.TryGetValue(rec.CountryCode, out var countryId)) continue;
            if (existingCities.Contains($"{countryId}:{rec.Name}")) continue;

            toAdd.Add(new City
            {
                CountryId  = countryId,
                Name       = rec.Name,
                Latitude   = rec.Coordinates?.Lat ?? 0,
                Longitude  = rec.Coordinates?.Lon ?? 0
            });
        }

        db.Cities.AddRange(toAdd);
        await db.SaveChangesAsync();
        return toAdd.Count;
    }

    private async Task<GeoPage> FetchCityPageAsync(int offset)
    {
        var where = Uri.EscapeDataString("population > 15000");
        var url   = $"{CitiesDataset}?select=name%2Ccountry_code%2Ccoordinates&where={where}&limit=100&offset={offset}";
        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new GeoPage(0, []);
            return await response.Content.ReadFromJsonAsync<GeoPage>() ?? new GeoPage(0, []);
        }
        catch
        {
            return new GeoPage(0, []);
        }
    }

    // --- External API response models ---

    private record RestCountry(RestCountryName Name, string Cca2);
    private record RestCountryName(string Common);

    private record GeoPage(
        [property: JsonPropertyName("total_count")] int TotalCount,
        [property: JsonPropertyName("results")]     List<GeoCityRecord> Results);

    private record GeoCityRecord(
        [property: JsonPropertyName("name")]         string Name,
        [property: JsonPropertyName("country_code")] string CountryCode,
        [property: JsonPropertyName("coordinates")]  GeoCoordinates? Coordinates);

    private record GeoCoordinates(
        [property: JsonPropertyName("lat")] double Lat,
        [property: JsonPropertyName("lon")] double Lon);
}
