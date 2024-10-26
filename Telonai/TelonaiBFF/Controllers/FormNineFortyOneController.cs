namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class FormNineFortyOneController : ControllerBase
{
    private readonly IFormNineFortyOneService _service;
    private readonly IMapper _mapper;

    public FormNineFortyOneController(IFormNineFortyOneService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(_mapper.Map<FormNineFortyOneModel>(item));
    }
    [HttpGet("current")]
    public IActionResult GetCurrent941FormsAsync()
    {
        var items = _service.GetCurrent941FormsAsync();
        return Ok(_mapper.Map<IList<FormNineFortyOneModel>>(items));
    }
    [HttpPost]
    public IActionResult Create()
    {
        var item = _service.CreateAsync();
        return Ok();
    }

}