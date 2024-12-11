namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class WorkScheduleController : ControllerBase
{
    private readonly IWorkScheduleService _workScheduleService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public WorkScheduleController(IWorkScheduleService ScheduleService, IScopedAuthorization scopedAuthorization)
    {
        _workScheduleService = ScheduleService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet]
    public IActionResult GetCurrentForUser()
    {
        var email = Request.HttpContext.User?.Claims.First(e=>e.Type=="email").Value;
        var Schedule = _workScheduleService.GetCurrentForUser(email);
        return Ok(Schedule);
    }


    [HttpGet("job/{jobId}/fromDate/{fromDate}/toDate/{toDate}")]
    public IActionResult GetByJobAndDateForUser(int jobId, [FromQuery(Name = "fromDate")] DateOnly fromDate, [FromQuery(Name = "toDate")] DateOnly toDate)
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        var Schedule = _workScheduleService.GetReport(email, jobId, fromDate, toDate);
        return Ok(Schedule);
    }

    [HttpGet("job/{jobId}")]
    public IActionResult GetCurrentForUser(int jobId)
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.User, jobId);

        var Schedule = _workScheduleService.GetCurrentForUser(email, jobId);
        return Ok(Schedule);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _workScheduleService.GetById(id);
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, user.JobId);
        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromBody] WorkScheduleRequestModel model)
    {
        var pattern = "/((1[0-2]|0?[1-9]):([0-5][0-9]) ?([AaPp][Mm]))/\r\n";

        if( Regex.IsMatch(model.StartTime, pattern))
            throw new AppException("Invalid schedule start-time");
        if (Regex.IsMatch(model.EndTime, pattern))
            throw new AppException("Invalid schedule end-time");

        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, model.JobId);

        _workScheduleService.Create(model.PersonId,model.JobId,model.ScheduledDate, model.StartTime,model.EndTime);
        return Ok(new { message = "Schedule updated." });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] WorkScheduleModel model)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.User, model.JobId);
        _workScheduleService.Update(id, model);
        return Ok(new { message = "Schedule updated." });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.Admin);

        _workScheduleService.Delete(id);
        return Ok(new { message = "Schedule deleted." });
    }
}