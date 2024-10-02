namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous()]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService<InvitationModel, Invitation> _service;
    private readonly ILogger<InvitationsController> _logger;
    public InvitationsController(IInvitationService<InvitationModel, Invitation> service, ILogger<InvitationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var result = _service.GetById(id);
        ScopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, result.JobId.Value);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("jobs/{id}")]
    public IActionResult GetByJobId(int id)
    {
        var result = _service.GetByJobId(id);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("companies/{id}/status")]
    public IActionResult GetStatusByCompanyId(int id)
    {
        var result = _service.GetStatusByCompanyId(id);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("emails/{email}")]
    public IActionResult GetByInviteeEmail(string email)
    {
        var result = _service.GetByInviteeEmail(email);
        return Ok(result);
    }


    [HttpPost("validate")]
    public IActionResult GetByActivationCodeAndEmail([FromBody] InvitationRequestModel model)
    {
        var codeRegEx = new Regex(@"^[a-zA-Z0-9\s,]*$");
        if (model.code.Length != 7 && !codeRegEx.IsMatch(model.code))
            throw new InvalidDataException();

        if (!InputValidator.IsValidEmail(model.Email))
            throw new InvalidDataException();

        var result = _service.GetAllByActivaionCodeAndInviteeEmail(model.code,model.Email);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public IActionResult InviteEmployee([FromBody] InvitationModel model)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, model.Employment.CompanyId);
        _service.CreateAsync(model,true);        
        return Ok();
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpPost("employer")]
    public IActionResult InviteEmployer([FromBody] InvitationModel model)
    {
        _service.CreateAsync(model,false);
        return Ok(new { message = "Employer Invited" });
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteAsync(id);
        return Ok(new { message = "Invitation deleted" });
    }
}