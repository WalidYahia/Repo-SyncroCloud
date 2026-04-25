using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(ILocationService service, ILocationSyncService syncService) : ApiControllerBase
{
    // --- Countries ---

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries() =>
        Ok(await service.GetAllCountriesAsync());

    [HttpGet("countries/{id:int}")]
    public async Task<IActionResult> GetCountry(int id)
    {
        var result = await service.GetCountryByIdAsync(id);
        return result is null ? ResourceNotFound("Country", id) : Ok(result);
    }

    [HttpPost("countries")]
    public async Task<IActionResult> CreateCountry(CreateCountryDto dto)
    {
        var result = await service.CreateCountryAsync(dto);
        return CreatedAtAction(nameof(GetCountry), new { id = result.Id }, result);
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var deleted = await service.DeleteCountryAsync(id);
        return deleted ? NoContent() : ResourceNotFound("Country", id);
    }

    // --- Cities ---

    [HttpGet("countries/{countryId:int}/cities")]
    public async Task<IActionResult> GetCities(int countryId) =>
        Ok(await service.GetCitiesByCountryAsync(countryId));

    [HttpGet("cities/{id:int}")]
    public async Task<IActionResult> GetCity(int id)
    {
        var result = await service.GetCityByIdAsync(id);
        return result is null ? ResourceNotFound("City", id) : Ok(result);
    }

    [HttpPost("cities")]
    public async Task<IActionResult> CreateCity(CreateCityDto dto)
    {
        var result = await service.CreateCityAsync(dto);
        return CreatedAtAction(nameof(GetCity), new { id = result.Id }, result);
    }

    [HttpPut("cities/{id:int}")]
    public async Task<IActionResult> UpdateCity(int id, UpdateCityDto dto)
    {
        var result = await service.UpdateCityAsync(id, dto);
        return result is null ? ResourceNotFound("City", id) : Ok(result);
    }

    [HttpDelete("cities/{id:int}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var deleted = await service.DeleteCityAsync(id);
        return deleted ? NoContent() : ResourceNotFound("City", id);
    }

    // --- Sync ---

    [HttpPost("sync")]
    public async Task<IActionResult> Sync() =>
        Ok(await syncService.SyncAsync());
}
