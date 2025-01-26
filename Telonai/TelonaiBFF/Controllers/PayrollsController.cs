namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[AllowAnonymous()]
public class PayrollsController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public PayrollsController(IPayrollService payrollService, IScopedAuthorization scopedAuthorization)
    {
        _payrollService = payrollService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet("companies/{companyId}/current")]
    [Authorize]
    public IActionResult GetCurrentPayroll(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetCurrentPayroll(companyId);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/previous")]
    [Authorize]
    public IActionResult GetPreviousPayroll(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetPreviousPayroll(companyId);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/report")]
    [Authorize]
    public IActionResult GetByCompanyAndTimeForUser(int companyId, [FromQuery(Name = "startTime")] DateOnly startTime, [FromQuery(Name = "endTime")] DateOnly endTime)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetReport(companyId, startTime, endTime);
        return Ok(payroll);
    }

    [HttpGet("companies/{companyId}/{count}")]
    [Authorize]
    public IActionResult GetByCompanyAndCount(int companyId, int count)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var payroll = _payrollService.GetLatestByCount(companyId, count);
        return Ok(payroll);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetById(int id)
    {
        var item = _payrollService.GetById(id);

        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, item.CompanyId);
        return Ok(item);
    }


    [HttpGet("{payrollId}/Summary")]
    [Authorize]
    public async Task<IActionResult> GetPayrollSummaryById(int payrollId)
    {
        var payrollSummary = await _payrollService.GetPayrollSummanryByPayrollId(payrollId);
        if(payrollSummary != null)
        {
            return Ok(payrollSummary);
        }
        else
        {
            var noPayrollSummaryFoundMessage = new
            {
                message = "There is no payroll summary"
            };
            var json = JsonSerializer.Serialize(noPayrollSummaryFoundMessage);
            return NotFound(json);
        }
    }

    [HttpPost("companies/{companyId}")]
    [Authorize]
    public IActionResult Create(int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        _payrollService.Create(companyId);
        return Ok(new { message = "Payroll created." });
    }

    [HttpPost("generate")]
    [Authorize]
    public IActionResult CreateNextPayrollForAll()
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var countryId = 2;

        _ = _payrollService.CreateNextPayrollForAll(countryId);
        _ = _payrollService.CreateNextPaystubForAllCurrentPayrollsAsync();
        return Ok("Invocation of Payroll generation and Paystub generation completed");
    }

    [HttpPut("{id}/companies/{companyId}")]
    [Authorize]
    public IActionResult Update(int id, int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        _payrollService.Update(id,companyId);
        return Ok(new { message = "Payrolls updated." });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _payrollService.Delete(id);
        return Ok(new { message = "Payroll deleted." });
    }


    [HttpPost("frequency/{frequency}/firstPaycheckDate")]
    [AllowAnonymous]
    public IActionResult GetFirstPaycheckDate([FromBody]DateOnly startDate, PayrollScheduleTypeModel frequency)
    {
        var item = _payrollService.GetFirstPaycheckDate(frequency, startDate, 2);
        return Ok(item);
    }
}