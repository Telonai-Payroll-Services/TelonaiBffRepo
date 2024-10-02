namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;


[ApiController]
[Route("[controller]")]
[Authorize()]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _service;
    private readonly IMapper _mapper;

    public CountriesController(ICountryService countryService, IMapper mapper)
    {
        _service = countryService;
        _mapper = mapper;

    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var items = _service.GetAll();
        return Ok(_mapper.Map<IList<CountryModel>>(items));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<CountryModel>(item));
    }

}