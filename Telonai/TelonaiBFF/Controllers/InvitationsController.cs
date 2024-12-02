namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IScopedAuthorization _scopedAuthrorization;
    private readonly IEmployerSubscriptionService _employerSubscriptionservice;

    public InvitationsController(IInvitationService<InvitationModel, Invitation> service, 
        ILogger<InvitationsController> logger, IScopedAuthorization scopedAuthrorization, IEmployerSubscriptionService employerSubscriptionservice)
    {
        _service = service;
        _logger = logger;
        _scopedAuthrorization = scopedAuthrorization;
        _employerSubscriptionservice = employerSubscriptionservice;
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var result = _service.GetById(id);
        _scopedAuthrorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, result.JobId.Value);

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
        if (model.code.Length != 8 || !codeRegEx.IsMatch(model.code))
            throw new InvalidDataException("Invalid Activation Code");

        if (!InputValidator.IsValidEmail(model.Email))
            throw new InvalidDataException("Invalid Email");

        var result = _service.GetAllByActivaionCodeAndInviteeEmail(model.code,model.Email);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public IActionResult InviteEmployee([FromBody] InvitationModel model)
    {
        _scopedAuthrorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, model.Employment.CompanyId);
        _service.CreateAsync(model,true);        
        return Ok();
    }

    [HttpPost("employer/code/{code}")]
    public async Task<IActionResult> InviteEmployer([FromBody] EmployerInvitationModel model , string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length!=4 || !ushort.TryParse(code, out var agentCode))
        {
            var message = ($"Invalid Agent Code: {code}, Company: {model.Company}, TaxId: {model.TaxId}, " +
                $"Address: {model.Address}, FirstName: {model.FirstName}, LastName: {model.LastName}, " +
                $"Phone");
            return Ok(message);
        }

        var invitationModel = new InvitationModel
        {
            Email = model.Email,
            Company = model.Company,
            CountryId = 2,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneCountryCode = "+1",            
            TaxId = model.TaxId
        };

        var invitation = await _service.CreateAsync(invitationModel, false);

        var subscriptionModel = new EmployerSubscriptionModel
        { 
            AccountNumber = model.AccountNumber,
            AccountNumber2 = model.AccountNumber2,
            RoutingNumber = model.RoutingNumber,  
            BankAccountType= model.AccountType,
            City = model.City,
            State = model.State,
            Zip = model.Zip,
            //Phone=model.PhoneNumber,
            AgentCode = agentCode,
            CompanyAddress = model.Address,
            NumberOfEmployees = model.NumberOfEmployees, 
            SubscriptionType = model.SubscriptionType,
            InvitationId=invitation.Id,
        };

        await _employerSubscriptionservice.CreateAsync(subscriptionModel);

        return Ok();
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _service.DeleteAsync(id);
        return Ok(new { message = "Invitation deleted" });
    }
}