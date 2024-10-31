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
    private readonly IScopedAuthorization _scopedAuthorization;
    public TimecardUsaNoteController(ITimecardUsaNoteService timecardNoteService, IScopedAuthorization scopedAuthorization)
    {
        _timecardNoteService = timecardNoteService;
        _scopedAuthorization = scopedAuthorization;
    }


    [HttpGet("companies/{companyId}/payrolls/{payrollId}")]
    public async Task<IActionResult> GetByPayrollId(int companyId, int payrollId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecards = await _timecardNoteService.GetNotesByPayrollId(companyId,payrollId);
        return Ok(timecards);
    }

    [HttpPost("companies/{companyId}")]
    public async Task<IActionResult> GetByTimeCardIds(int companyId, [FromBody] List<int> timecardIds)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        var timecards = await _timecardNoteService.GetNotesByTimeCardIds(companyId, timecardIds);
        return Ok(timecards);
    }


    [HttpPut("{id}")]
    public IActionResult Update(int id, TimecardUsaNoteModel model)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.Admin);
        _timecardNoteService.Update(id, model);
        return Ok(new { message = "Timecard updated." });
    }

    [HttpPut("jobs/{jobId}")]
    public IActionResult Update(int jobId, [FromBody]List<TimecardUsaNoteModel> models)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, jobId);

        _timecardNoteService.Update(models);
        return Ok(new { message = "Timecard updated." });
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _timecardNoteService.Delete(id);
        return Ok(new { message = "Timecard deleted." });
    }
    [HttpPost("job/{jobId}")]
    public IActionResult CreateTimeCard(int jobId, [FromBody] List<TimecardUsaNoteModel> models)
    {

        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, jobId);

        _timecardNoteService.Create(models);
        return Ok(new { message = "Timecard created." });
    }
}