namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class CitiesController : ControllerBase
{
    private readonly ICityService _service;
    private readonly IMapper _mapper;

    public CitiesController(ICityService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("countries/{id}")]
    public IActionResult GetByCountryId(int id)
    {
        var items = _service.GetByCountryId(id);
        return Ok(_mapper.Map<IList<CityModel>>(items));
    }
    [HttpGet("states/{id}")]
    public IActionResult GetByStateId(int id)
    {
        var items = _service.GetByStateId(id);
        return Ok(_mapper.Map<IList<CityModel>>(items));
    }
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<CityModel>(item));
    }

}