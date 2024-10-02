namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "SystemAdmin")]
public class StatesController : ControllerBase
{
    private IStateService _service;
    private readonly IMapper _mapper;

    public StatesController(IStateService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;

    }

    [HttpGet("countries/{id}")]
    public IActionResult GetByCountryId(int id)
    {
        var items = _service.GetByCountryId(id);
        return Ok(_mapper.Map<IList<StateModel>>(items));
    }
    [HttpGet("{name}/countries/{countryId}")]
    public IActionResult GetByName(string name, int countryId)
    {
        var items = _service.GetByName(name, countryId);
        return Ok(_mapper.Map<StateModel>(items));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<StateModel>(item));
    }

}