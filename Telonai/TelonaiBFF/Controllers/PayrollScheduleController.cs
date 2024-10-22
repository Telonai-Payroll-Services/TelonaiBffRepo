namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class PayrollScheduleController : ControllerBase
{
    private readonly IPayrollScheduleService _PayrollScheduleService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public PayrollScheduleController(IPayrollScheduleService PayrollScheduleService, IScopedAuthorization scopedAuthorization)
    {
        _PayrollScheduleService = PayrollScheduleService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet("companies/{companyId}")]
    public IActionResult GetPayrollSchedule(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var PayrollSchedule = _PayrollScheduleService.GetLatestByCompanyId(companyId);
        return Ok(PayrollSchedule);
    }


    [HttpGet("companies/{companyId}/all")]
    public IActionResult GetAllPayrollSchedules(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var PayrollSchedule = _PayrollScheduleService.GetByCompanyId(companyId);
        return Ok(PayrollSchedule);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _PayrollScheduleService.GetById(id);
        return Ok(item);
    }

    [HttpPost()]
    public IActionResult Create([FromBody]PayrollScheduleModel model)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, model.CompanyId);
        _PayrollScheduleService.Create(model);
        return Ok(new { message = "Payroll Schedule created." });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id,PayrollScheduleModel model)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, model.CompanyId);
        _PayrollScheduleService.Update(id,  model);
        return Ok(new { message = "Payroll Schedule updated." });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        _PayrollScheduleService.Delete(id);
        return Ok(new { message = "Payroll Schedule deleted." });
    }
}