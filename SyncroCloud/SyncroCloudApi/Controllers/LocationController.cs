using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(ILocationService service) : ControllerBase
{
    // --- Countries ---

    [HttpGet("countries")]
    public async Task<ActionResult<List<CountryDto>>> GetCountries() =>
        Ok(await service.GetAllCountriesAsync());

    [HttpGet("countries/{id:int}")]
    public async Task<ActionResult<CountryDto>> GetCountry(int id)
    {
        var result = await service.GetCountryByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("countries")]
    public async Task<ActionResult<CountryDto>> CreateCountry(CreateCountryDto dto)
    {
        var result = await service.CreateCountryAsync(dto);
        return CreatedAtAction(nameof(GetCountry), new { id = result.Id }, result);
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var deleted = await service.DeleteCountryAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // --- Cities ---

    [HttpGet("countries/{countryId:int}/cities")]
    public async Task<ActionResult<List<CityDto>>> GetCities(int countryId) =>
        Ok(await service.GetCitiesByCountryAsync(countryId));

    [HttpGet("cities/{id:int}")]
    public async Task<ActionResult<CityDto>> GetCity(int id)
    {
        var result = await service.GetCityByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("cities")]
    public async Task<ActionResult<CityDto>> CreateCity(CreateCityDto dto)
    {
        var result = await service.CreateCityAsync(dto);
        return CreatedAtAction(nameof(GetCity), new { id = result.Id }, result);
    }

    [HttpPut("cities/{id:int}")]
    public async Task<ActionResult<CityDto>> UpdateCity(int id, UpdateCityDto dto)
    {
        var result = await service.UpdateCityAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("cities/{id:int}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var deleted = await service.DeleteCityAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
