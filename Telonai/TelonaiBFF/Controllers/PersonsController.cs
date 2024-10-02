namespace TelonaiWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class PersonsController : ControllerBase
{
    private readonly IPersonService<PersonModel, Person> _service;

    public PersonsController(IPersonService<PersonModel, Person> service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyId}")]
    public IActionResult GetByCompanyId(int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var users = _service.GetByCompanyId(companyId);
        return Ok(users);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        ScopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var profiles = _service.Get();
        return Ok(profiles);
    }
    [HttpGet("{email}")]
    public IActionResult GetByEmail(string email)
    {
        var user = _service.GetByEmailAsync(email)?.Result;
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);


        return Ok(user);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _service.GetById(id);
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);

        return Ok(user);
    }


    [HttpGet("{id}/details")]
    public IActionResult GetDetailsById(int id)
    {
        var user = _service.GetDetailsById(id);
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create(PersonModel model)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User,model.CompanyId);

        _service.CreateAsync(model);
        return Ok(new { message = "Account created" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, PersonModel model)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, model.CompanyId);

        _service.Update(id, model);
        return Ok(new { message = "Account updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        _service.DeleteAsync(id);
        return Ok(new { message = "Account deleted" });
    }
}