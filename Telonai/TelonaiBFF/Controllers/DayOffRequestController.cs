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
public class DayOffRequestController : ControllerBase
{

    private readonly IDayOffRequestService<DayOffRequestModel, DayOffRequest> _service;
    private readonly ILogger<DayOffRequestController> _logger;
    private readonly IScopedAuthorization _scopedAuthrorization;

    public DayOffRequestController(IDayOffRequestService<DayOffRequestModel, DayOffRequest> service, 
        ILogger<DayOffRequestController> logger, IScopedAuthorization scopedAuthrorization)
    {
        _service = service;
        _logger = logger;
        _scopedAuthrorization = scopedAuthrorization;
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _service.GetById(id);
        //TO Do: _scopedAuthrorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, result.JobId);

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
    [HttpPost]
    public IActionResult CreateDayOffRequest([FromBody] DayOffRequestModel model)
    {
       //TO Do: Fix this later _scopedAuthrorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, model.EmploymentId);
        _service.CreateAsync(model);        
        return Ok();
    }


    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.DeleteAsync(id);
        return Ok(new { message = "Day-off Request deleted" });
    }
}