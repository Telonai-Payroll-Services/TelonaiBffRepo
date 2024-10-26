namespace TelonaiWebApi.Controllers;

using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class OtherIncomeController : ControllerBase
{
    private readonly IOtherMoneyReceivedService _otherIncomeService;
    private readonly IScopedAuthorization _scopedAuthorization;

    public OtherIncomeController(IOtherMoneyReceivedService otherIncomeService, IScopedAuthorization scopedAuthorization)
    {
        _otherIncomeService = otherIncomeService;
        _scopedAuthorization = scopedAuthorization;
    }


    [HttpGet("payrolls/{payrollId}")]
    public IActionResult GetCurrentByPayrollId(int payrollId)
    {
        var OtherIncomes = _otherIncomeService.GetByPayrollId(payrollId,  out var companyId);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        return Ok(OtherIncomes);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _otherIncomeService.GetById(id);

        //_scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, item.PayStub.Payroll.CompanyId);
        return Ok(item);
    }

    [HttpPost()]
    public IActionResult Create([FromBody]OtherMoneyReceivedModel model)
    {
        _otherIncomeService.Create(model);
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody]OtherMoneyReceivedModel model)
    {
        var stub = _otherIncomeService.GetById(id);
        //_scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, stub.PayStub.Payroll.CompanyId);

        _otherIncomeService.Update(model);
        return Ok(new { message = "OtherIncome updated." });
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        _otherIncomeService.Delete(id);
        return Ok(new { message = "OtherIncome deleted." });
    }
}