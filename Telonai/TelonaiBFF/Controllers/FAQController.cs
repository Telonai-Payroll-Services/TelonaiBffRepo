namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[Route("[controller]")]
[ApiController]
[Authorize()]
[AllowAnonymous]
public class FAQController : ControllerBase
{
    private readonly IFAQService _service;
    private readonly IMapper _mapper;

    private readonly IScopedAuthorization _scopedAuthorization;

    public FAQController(IFAQService faqService, IMapper mapper, IScopedAuthorization scopedAuthorization)
    {
        _service = faqService;
        _mapper = mapper;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var items = _service.Get();
        return Ok(items);
    }

    [HttpGet("contact")]
    public IActionResult GetFAQAndContact()
    {
        var response = _service.GetFAQAndContact();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _service.GetById(id);
        return Ok(item);
    }

    [HttpPost]
    public IActionResult Create(FAQModel faq)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        _service.CreateAsync(faq);
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, FAQModel faq)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        _service.Update(id, faq);
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        _service.Delete(id);
        return Ok();
    }

}
