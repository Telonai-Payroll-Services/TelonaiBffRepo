namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class DayOffRequestController : ControllerBase
{

    private readonly IDayOffRequestService<DayOffRequestModel, DayOffRequest> _service;
    private readonly ILogger<DayOffRequestController> _logger;
    private readonly IScopedAuthorization _scopedAuthrorization;
    private readonly IDayOffTypeService _dayOffTypeService;

    public DayOffRequestController(IDayOffRequestService<DayOffRequestModel, DayOffRequest> service,
        ILogger<DayOffRequestController> logger, IScopedAuthorization scopedAuthrorization, IDayOffTypeService dayOffService)
    {
        _service = service;
        _logger = logger;
        _scopedAuthrorization = scopedAuthrorization;
        _dayOffTypeService = dayOffService;
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("employment/{id}")]
    public IActionResult GetByEmploymentId(int id)
    {
        var result = _service.GetByEmploymentId(id);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("person/{id}")]
    public IActionResult GetByPersonId(int id)
    {
        var result = _service.GetByPersonId(id);
        return Ok(result);
    }


    [Authorize]
    [HttpGet("company/{id}")]
    public IActionResult GetByCompanyId(int id)
    {
        _scopedAuthrorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, id);

        var result = _service.GetByCompanyId(id);
        return Ok(result);
    }


    [Authorize]
    [HttpPost]
    public IActionResult CreateDayOffRequest([FromBody] DayOffRequestModel model)
    {
        if (model.FromDate.CompareTo(model.ToDate) > 0)
            throw new AppException("From date should be before To date");

        if (model.FromDate.CompareTo(DateOnly.FromDateTime(DateTime.Now)) < 0)
            throw new AppException("Requesting a day off for past dates is not allowed.");

        _service.CreateAsync(model);
        return Ok();
    }


    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.DeleteAsync(id);
        return Ok(new { message = "Day-off Request deleted" });
    }

    [HttpGet("GetAllDayOffTypes")]
    public IActionResult GetAllDayOffTypes()
    {
        var dayOffTypes = _dayOffTypeService.GetAllDayOffType();
        if (dayOffTypes != null)
        {
            return Ok(dayOffTypes);
        }
        else
        {
            return NotFound("There are no day off types registered.");
        }
    }

    [HttpGet("GetDayOffTypesById/{id}")]
    public IActionResult GetDayOffTypeById(int id)
    {
        var dayOffType = _dayOffTypeService.GetDayOffTypeById(id);
        if (dayOffType != null)
        {
            return Ok(dayOffType);
        }
        else
        {
            return NotFound("There is any day off types registered.");
        }
    }

    [HttpPut("Approve")]
    public async Task<IActionResult> ApproveDayOffRequest([FromBody] ApproveDayOffRequest approveDayOffRequest)
    {
        var dayOffRequest = _service.GetDayOffRequestDetail(approveDayOffRequest.Id);
        if (dayOffRequest != null)
        {
            //Check whether the logged user is an admin of a company.
            _scopedAuthrorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, dayOffRequest.Employment.JobId);
            var result = await _service.ApproveDayOffRequest(approveDayOffRequest, dayOffRequest);
            if (result)
            {
                string message = approveDayOffRequest.IsApproved ? "The dayoff request of the employee is approved successfully":
                                                                  "The dayoff request of the employee is rejected successfully";
                return Ok(message);
                return Ok(message);
            }
            else
            {
                return BadRequest();
            }
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPut("Cancel/{id}")]
    public async Task<IActionResult> CancelDayOffRequest(int id)
    {
        var result = await _service.CancelDayOffRequest(id);
        if (result)
        {
            return Ok("Your dayOff request is canceled successfully");
        }
        else
        {
            return BadRequest();
        }
    }
}