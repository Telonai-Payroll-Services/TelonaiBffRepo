namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class FormNineFortyFourController : ControllerBase
{
    private readonly IFormNineFortyFourService _service;
    private readonly IMapper _mapper;

    public FormNineFortyFourController(IFormNineFortyFourService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<FormNineFortyFourModel>(item));
    }
    [HttpGet("current")]
    public IActionResult GetCurrent944FormsAsync()
    {
        var items = _service.GetCurrent944FormsAsync();
        return Ok(_mapper.Map<IList<FormNineFortyFourModel>>(items));
    }

    [HttpGet("previous/{year}")]
    public IActionResult GetPrevious944FormsAsync(int year)
    {
        var items = _service.GetPrevious944FormsAsync(year);
        return Ok(_mapper.Map<IList<FormNineFortyFourModel>>(items));
    }
    [HttpPost]
    public IActionResult Create()
    {
        var item = _service.CreateAsync();
        return Ok();
    }

}