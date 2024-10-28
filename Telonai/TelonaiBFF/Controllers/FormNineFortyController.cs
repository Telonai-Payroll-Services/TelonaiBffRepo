namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class FormNineFortyController : ControllerBase
{
    private readonly IFormNineFortyService _service;
    private readonly IMapper _mapper;

    public FormNineFortyController(IFormNineFortyService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<FormNineFortyModel>(item));
    }
    [HttpGet("current")]
    public IActionResult GetCurrent940FormsAsync()
    {
        var items = _service.GetCurrent940FormsAsync();
        return Ok(_mapper.Map<IList<FormNineFortyModel>>(items));
    }

    [HttpGet("previous/{year}")]
    public IActionResult GetPrevious940FormsAsync(int year)
    {
        var items = _service.GetPrevious940FormsAsync(year);
        return Ok(_mapper.Map<IList<FormNineFortyModel>>(items));
    }
    [HttpPost]
    public IActionResult Create()
    {
        var item = _service.CreateAsync();
        return Ok();
    }

}