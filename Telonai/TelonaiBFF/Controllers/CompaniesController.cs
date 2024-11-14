namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService<CompanyModel,Company> _service;    
    private readonly IUserService _userService;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly IInvitationService<InvitationModel, Invitation> _invitationService;
    private readonly IEmploymentService<EmploymentModel, Employment> _employmentService;
    private readonly IJobService<JobModel, Job> _jobService;
    private readonly IScopedAuthorization _scopedAuthorization;

    public CompaniesController(ICompanyService<CompanyModel, Company> companyService, IUserService userService, IPersonService<PersonModel, Person> personService
        , IInvitationService<InvitationModel, Invitation> invitationService, IEmploymentService<EmploymentModel, 
            Employment> employmentService, IJobService<JobModel, Job> jobService, IScopedAuthorization scopedAuthorization)
    {
        _service = companyService;
        _userService = userService;
        _personService = personService;
        _invitationService = invitationService;
        _employmentService = employmentService;
        _jobService = jobService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult GetAll()
    {
        var companies = _service.Get();
        return Ok(companies);
    }

    [HttpGet("{id}")]
    [Authorize()]
    public IActionResult GetById(int id)
    {
        var job = _service.GetById(id);
        return Ok(job);
    }

    [HttpGet("jobs/{id}")]
    [Authorize()]
    public IActionResult GetJobsByCompanyId(int id)
    {
        var jobs = _service.GetJobsById(id);
        return Ok(jobs);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]CompanyRegistrationRequestModel model)
    {
        if (!ModelState.IsValid )
            return BadRequest();

        var invitation = _invitationService.GetByActivaionCodeAndInviteeEmail(model.Manager.ActivationCode, model.Manager.Email, model.Company.TaxId);
        model.Company.TaxId=invitation.TaxId;

        var company = MakeCompanyModel(model.Company);

        var createdCompany = await _service.CreateAsync(company);

        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _userService.SignUpAsync(model.Manager,UserRole.Admin, createdCompany.Id, 0);
        if (result.Succeeded || !result.Errors.Any())
        {            
            var person = await CreateProfileFromUserAsync(model.Manager, createdCompany.Id);
            var job = MakeJobModel(createdCompany);

            var createdJob = await _jobService.CreateAsync(job,model.Manager.Username,model.Manager.Email);

            var emp = await CreateEmploymment(createdJob.Id, person.Id, createdCompany.Id,(int)SignUpStatusTypeModel.CompanyProfileCreationCompleted);

            invitation.ExpirationDate = DateTime.UtcNow;
            await _invitationService.UpdateAsync(invitation);

            var loginResult = new LoginResult
            {
                Employments = new List<EmploymentModel> { emp },
                FullName = $"{person.FirstName} {person.LastName}"
            };

            return Ok(loginResult);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description.Replace("Cognito",""));

        return BadRequest();
    }

    [HttpPut("{id}")]
    [Authorize()]
    public async Task<IActionResult> Update(int id, CompanyModel model)
    {
        await _service.UpdateAsync(id, model);
        return Ok(new { message = "Account updated" });
    }

    [HttpDelete("{id}")]
    [Authorize()]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "Account deleted" });
    }

    [HttpGet("{id}/summary")]
    public IActionResult GetSummary(int id)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, id);

        var result = _service.GetSummary(id,4);

        return Ok(result);
    }

    private async Task<PersonModel> CreateProfileFromUserAsync(User user, int companyId)
    {
        var p = new Person
        {
            FirstName = user.Firstname,
            LastName = user.Lastname,
            MiddleName = user?.Middlename,
            Email = user?.Email,
            MobilePhone = user?.MobilePhone,
            CompanyId = companyId
        };

        return await _personService.CreateAsync(p);
    }
    private async Task<EmploymentModel> CreateEmploymment(int jobId, int personId,int companyId, int statusId)
    {
        var emp = new Employment
        {
            JobId = jobId,
            IsPayrollAdmin = true,
            PersonId = personId,
            SignUpStatusTypeId=statusId
        };
        return await _employmentService.CreateAsync(emp, companyId);
    }

    private static CompanyModel MakeCompanyModel(CompanyRequestModel model)
    {
        return new CompanyModel
        {
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            AddressLine3 = model.AddressLine3,
            BusinessType = model.BusinessType,
            ZipcodeId = model.ZipCodeId,
            Name = model.Name,
            RegistrationNumber = model.RegistrationNumber,
            TaxId = model.TaxId
        };
    }

    private static JobModel MakeJobModel(Company company)
    {
        return new JobModel
        {
            AddressLine1 = company.AddressLine1,
            AddressLine2 = company.AddressLine2,
            AddressLine3 = company.AddressLine3,
            ZipcodeId = company.ZipcodeId,
            CompanyId = company.Id,
            LocationName = company.AddressLine1
        }; 
    }
}