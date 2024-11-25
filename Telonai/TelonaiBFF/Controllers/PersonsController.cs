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
    private readonly IScopedAuthorization _scopedAuthorization;
    public PersonsController(IPersonService<PersonModel, Person> service, IScopedAuthorization scopedAuthorization)
    {
        _service = service;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet("companies/{companyId}")]
    public IActionResult GetByCompanyId(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var users = _service.GetByCompanyId(companyId);
        return Ok(users);
    }

    [HttpGet("companies/{companyId}/incompleteInine")]
    public IActionResult GetIncompleteInineByCompanyId(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var users = _service.GetIncompleteInineByCompanyId(companyId);
        return Ok(users);
    }
    
    [HttpGet]
    public IActionResult GetAll()
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var profiles = _service.Get();
        return Ok(profiles);
    }
    [HttpGet("email/{email}")]
    public IActionResult GetByEmail(string email)
    {
        var user = _service.GetByEmailAsync(email)?.Result;
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);


        return Ok(user);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _service.GetById(id);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);

        return Ok(user);
    }


    [HttpGet("{id}/details")]
    public IActionResult GetDetailsById(int id)
    {
        var user = _service.GetDetailsById(id);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, user.CompanyId);

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create(PersonModel model)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User,model.CompanyId);

        _service.CreateAsync(model);
        return Ok(new { message = "Account created" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, PersonModel model)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, model.CompanyId);

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