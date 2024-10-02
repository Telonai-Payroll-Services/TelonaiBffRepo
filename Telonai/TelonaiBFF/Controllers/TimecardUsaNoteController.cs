namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class TimecardUsaNoteController : ControllerBase
{
    private readonly ITimecardUsaNoteService _timecardNoteService;
    public TimecardUsaNoteController(ITimecardUsaNoteService timecardNoteService)
    {
        _timecardNoteService = timecardNoteService;
    }


    [HttpGet("companies/{companyId}/payrolls/{payrollId}")]
    public async Task<IActionResult> GetByPayrollId(int companyId, int payrollId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecards = await _timecardNoteService.GetNotesByPayrollId(companyId,payrollId);
        return Ok(timecards);
    }

    [HttpPost("companies/{companyId}")]
    public async Task<IActionResult> GetByTimeCardIds(int companyId, [FromBody] List<int> timecardIds)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecards = await _timecardNoteService.GetNotesByTimeCardIds(companyId, timecardIds);
        return Ok(timecards);
    }


    [HttpPut("{id}")]
    public IActionResult Update(int id, TimecardUsaNoteModel model)
    {
        ScopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.Admin);
        _timecardNoteService.Update(id, model);
        return Ok(new { message = "Timecard updated." });
    }

    [HttpPut("jobs/{jobId}")]
    public IActionResult Update(int jobId, [FromBody]List<TimecardUsaNoteModel> models)
    {
        ScopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, jobId);

        _timecardNoteService.Update(models);
        return Ok(new { message = "Timecard updated." });
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        ScopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _timecardNoteService.Delete(id);
        return Ok(new { message = "Timecard deleted." });
    }
}