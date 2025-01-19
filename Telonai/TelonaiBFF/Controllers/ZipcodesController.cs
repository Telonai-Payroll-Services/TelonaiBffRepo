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
        if (items != null)
        {
            return Ok(_mapper.Map<List<ZipcodeModel>>(items));
        }
        else
        {
            return NotFound();
        }
    }
    [HttpGet("{code}/countries/{countryId}")]
    public IActionResult GetByZipCodeAndCountryId(string code, int countryId)
    {
        var results = _service.GetModelByZipcodeAndCountryId(code, countryId);
        if (results != null)
        {
            return Ok(results);
        }
        else 
        {
            return NotFound();      
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        if (item != null) 
        {
            return Ok(_mapper.Map<ZipcodeModel>(item));
        }
        else
        {
            return NotFound();
        }
        
    }
}