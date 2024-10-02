namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class EmploymentController : ControllerBase
{
    private readonly IEmploymentService<EmploymentModel, Employment> _service;
    public EmploymentController(IEmploymentService<EmploymentModel, Employment> service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);
        ScopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, result.JobId);

        return Ok(result);
    }

    [HttpPost("companies/{companyid}/completeadding")]
    public IActionResult InviteEmployee(int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var currentUserEmail = Request.HttpContext.User.Claims.First(e => e.Type == "email").Value;
      
        _service.CompleteAddingEmployees(currentUserEmail,companyId);        
        return Ok(new { message = "Employee Invited" });
    }
    [HttpPut("{id}/companies/{companyid}/terminate")]
    public IActionResult TerminateEmployee(int id, int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        _service.DeleteAsync(id, companyId);
        return Ok(new { message = "Employee Terminated" });
    }
}