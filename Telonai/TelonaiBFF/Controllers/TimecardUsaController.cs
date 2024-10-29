namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class TimecardUsaController : ControllerBase
{
    private readonly ITimecardUsaService _timecardService;
    private readonly ITimecardUsaNoteService _timecardNoteService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public TimecardUsaController(ITimecardUsaService timecardService, ITimecardUsaNoteService timecardNoteService, IScopedAuthorization scopedAuthorization)
    {
        _timecardService = timecardService;
        _timecardNoteService = timecardNoteService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet("open")]
    public IActionResult GetOpenedTimeCard()
    {
        var email = Request.HttpContext.User?.Claims.First(e=>e.Type=="email").Value;
        var timecard = _timecardService.GetOpenTimeCard(email);
        return Ok(timecard);
    }


    [HttpGet("job/{jobId}")]
    public IActionResult GetByJobAndTimeForUser(int jobId, [FromQuery(Name = "startTime")] DateTime startTime, [FromQuery(Name = "endTime")] DateTime endTime)
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        var timecard = _timecardService.GetReport(email, jobId, startTime, endTime);
        return Ok(timecard);
    }

    [HttpGet("self")]
    public IActionResult GetByTimeForUser([FromQuery(Name = "startTime")] DateTime startTime, [FromQuery(Name = "endTime")] DateTime endTime)
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        var timecard = _timecardService.GetReport(email, startTime, endTime);
        return Ok(timecard);
    }

    [HttpGet("companies/{companyId}/self/current")]
    public async Task<IActionResult> GetCurrentByTimeForUser(int companyId)
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        var timecard = await _timecardService.GetTimeCardsByPayrollSequenceAndEmail(email, 0, companyId);
        return Ok(timecard);
    }

    [HttpGet("companies/{companyId}/payrolls/{payrollId}")]
    public async Task<IActionResult> GetByPayrollId(int companyId, int payrollId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecards = await _timecardService.GetTimeCardsByPayrollId(companyId,payrollId);
        return Ok(timecards);
    }
    [HttpGet("companies/{companyId}/payrolls/{payrollId}/employees/{employeeId}")]
    public async Task<IActionResult> GetByPayrollIdAndEmployee(int companyId, int payrollId, int employeeId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecard = await _timecardService.GetTimeCardsByPayrollIdAndEmployee(companyId, payrollId,employeeId);
        return Ok(timecard);
    }

    [HttpGet("companies/{companyId}/payrolls/sequences/{seqId}/employee/{employeeId}")]
    public IActionResult GetByPayrollSequenceAndEmployee(int companyId, int seqId, int employeeId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecard = _timecardService.GetTimeCardsByPayrollSequenceAndEmployee(companyId, seqId,  employeeId);
        return Ok(timecard);
    }

    [HttpGet("job/{jobId}/generic")]
    public IActionResult GetByJobAndTime(int jobId, [FromQuery(Name = "startTime")] DateTime startTime, [FromQuery(Name = "endTime")] DateTime endTime)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, jobId);

        var timecard = _timecardService.GetReport(jobId, startTime, endTime);
        return Ok(timecard);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _timecardService.GetById(id);
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, user.JobId);
        return Ok(user);
    }

    [HttpPost("job/{jobId}")]
    public IActionResult Create(int jobId)
    {
        var email = Request.HttpContext.User?.Claims.FirstOrDefault(e => e.Type == "email")?.Value;
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.User, jobId);

        _timecardService.Create(email,jobId);
        return Ok(new { message = "Timecard updated." });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, TimecardUsaModel model)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, model.JobId);
        _timecardService.Update(id, model);
        return Ok(new { message = "Timecard updated." });
    }
      
    [HttpPut()]
    public IActionResult Update([FromBody] TimecardUsaUpdateModel model)
    {
        var jobId= model.TimecardUsaModels.First().JobId;
        var isValid = model.TimecardUsaModels.Any(e => e.JobId != jobId) ? throw new AppException("Invalid Request") : true;
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, jobId);

        _timecardService.Update(model.TimecardUsaModels);
        model.TimecardUsaNoteModels.ForEach(e => _timecardNoteService.Create(e));

        return Ok(new { message = "Timecard updated." });
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _timecardService.Delete(id);
        return Ok(new { message = "Timecard deleted." });
    }
}