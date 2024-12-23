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


    [HttpGet("paystubs/{paystubId}")]
    public IActionResult GetCurrentByPayrollId(int paystubId)
    {
        var OtherIncomes = _otherIncomeService.GetByPayStubId(paystubId,  out var companyId);
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

    [HttpPost("paystub/{paystubId}")]
    public async Task<IActionResult> Create(int paystubId, [FromBody]OtherMoneyReceivedModel model)
    {
        var isOtherIncomeSaved = await _otherIncomeService.CreateOrUpdate(paystubId,model);
        if(isOtherIncomeSaved)
        {
            return Ok("Additional earnings added successfully.");
        }
        else
        {
            return NotFound("Not able to add additional earnings")
;        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int paystubId, [FromBody]OtherMoneyReceivedModel model)
    {
       //No Authorization here. It is done in the service
        var result = await _otherIncomeService.CreateOrUpdate(paystubId,model);
        if(result)
        {
            return Ok(new { message = "Additional earnings updated successfully." });
        }
        else
        {
            return NotFound("Not able to update additional earnings");
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _otherIncomeService.Delete(id);
        if(result)
        {
            return Ok(new { message = "OtherIncome deleted." });
        }
        else
        {
            return NotFound("There is no other income registered under the provided id.");
        }
    }
}