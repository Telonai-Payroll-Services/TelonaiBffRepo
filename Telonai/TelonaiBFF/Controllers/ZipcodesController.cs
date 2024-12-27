namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class ZipcodesController : ControllerBase
{
    private readonly IZipcodeService _service;
    private readonly IMapper _mapper;

    public ZipcodesController(IZipcodeService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;

    }

    [HttpGet("cities/{id}")]
    public IActionResult GetByCityId(int id)
    {
        var items = _service.GetByCityId(id);
        return Ok(_mapper.Map<IList<ZipcodeModel>>(items));
    }
    [HttpGet("{code}/countries/{countryId}")]
    public IActionResult GetByZipCodeAndCountryId(string code, int countryId)
    {
        var results = _service.GetModelByZipcodeAndCountryId(code, countryId);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<ZipcodeModel>(item));
    }
}