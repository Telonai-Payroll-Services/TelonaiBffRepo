namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class PayrollsController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    public PayrollsController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("companies/{companyId}/current")]
    public IActionResult GetCurrentPayroll(int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetCurrentPayroll(companyId);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/previous")]
    public IActionResult GetPreviousPayroll(int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetPreviousPayroll(companyId);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/report")]
    public IActionResult GetByCompanyAndTimeForUser(int companyId, [FromQuery(Name = "startTime")] DateOnly startTime, [FromQuery(Name = "endTime")] DateOnly endTime)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetReport(companyId, startTime, endTime);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/{count}")]
    public IActionResult GetByCompanyAndCount(int companyId, int count)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetLatestByCount(companyId, count);
        return Ok(payroll);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _payrollService.GetById(id);

        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, item.CompanyId);
        return Ok(item);
    }

    [HttpPost("companies/{companyId}")]
    public IActionResult Create(int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        _payrollService.Create(companyId);
        return Ok(new { message = "Payroll created." });
    }

    [HttpPost("generate")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult CreateNextPayrollForAll()
    {
        var result = _payrollService.CreateNextPayrollForAll().Result;
        return Ok(new { message = $"{result} Payrolls generated." });
    }

    [HttpPut("{id}/companies/{companyId}")]
    public IActionResult Update(int id, int companyId)
    {
        ScopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        _payrollService.Update(id,companyId);
        return Ok(new { message = "Payrolls updated." });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        _payrollService.Delete(id);
        return Ok(new { message = "Payroll deleted." });
    }
}