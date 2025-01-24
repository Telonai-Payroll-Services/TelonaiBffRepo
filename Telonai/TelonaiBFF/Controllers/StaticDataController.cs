namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[Route("[controller]")]
[AllowAnonymous]
public class StaticDataController : ControllerBase
{
    private IStaticDataService _service;

    public StaticDataController(IStaticDataService service)
    {
        _service = service;
    }

    [HttpGet("countries")]
    public IActionResult GetAllCountries()
    {
        var result = _service.GetCountries();
        return Ok(result);
    }

    [HttpGet("countries/{countryId}")]
    public IActionResult GeCountryById(int countryId)
    {
        var result = _service.GetCountryById(countryId);
        return Ok(result);
    }

    [HttpGet("countries/{countryId}/states")]
    public IActionResult GetSates(int countryId)
    {
        var result = _service.GetStatesByCountryId(countryId);
        return Ok(result);
    }

    [HttpGet("states/{stateId}/cities")]
    public IActionResult GetCitiesByStateId(int stateId)
    {
        var result = _service.GetCitiesByStateId(stateId);
        return Ok(result);
    }


    [HttpGet("cities/{id}")]
    public IActionResult GeCityById(int id)
    {
        var result = _service.GetCityById(id);
        return Ok(result);
    }

    [HttpGet("states/{id}")]
    public IActionResult GeStateById(int id)
    {
        var result = _service.GetStateById(id);
        return Ok(result);
    }


    [HttpGet("countrys/{countryId}/zipcodes/{code}/cities")]
    public IActionResult GetCitiesByCodeAndCountryId(int countryId, string code)
    {
        var result = _service.GetZipcodesByCodeAndCountryId(code, countryId);
        return Ok(result);
    }

    [HttpGet("roleTypes")]
    public IActionResult GetAllRoleTypes()
    {
        var result = _service.GetRoleTypes();
        return Ok(result);
    }
    [HttpGet("businessTypes")]
    public IActionResult GetAllBusinessTypes()
    {
        var result = _service.GetBusinessTypes();
        return Ok(result);
    }

    [HttpGet("county/{id}")]
    public IActionResult GetCountyById(int id)
    {
        var result = _service.GetCountyById(id);
        return Ok(result);
    }

    [HttpGet("state/{stateId}/counties")]
    public IActionResult GetCountyByStateId(int stateId)
    {
        var result = _service.GetCountyByStateId(stateId);
        return Ok(result);
    }

    [HttpGet("name/{name}/state/{stateId}/county")]
    public IActionResult GetCountyByStateId(string name, int stateId)
    {
        var result = _service.GetCountyByNameAndStateId(name, stateId);
        return Ok(result);
    }
}