namespace TelonaiWebApi.Controllers;

using Amazon.Auth.AccessControlPolicy;
using Amazon.Util.Internal;
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
public class PayStubController : ControllerBase
{
    private readonly IPayStubService _PayStubService;
    private readonly IScopedAuthorization _scopedAuthorization;

    public PayStubController(IPayStubService PayStubService, IScopedAuthorization scopedAuthorization)
    {
        _PayStubService = PayStubService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet("companies/{companies}")]
    public IActionResult GetCurrentPayStubs(int companyId)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var PayStub = _PayStubService.GetCurrentByCompanyId(companyId);
        return Ok(PayStub);
    }

    [HttpGet("companies/{companyId}/persons/{personId}")]
    public IActionResult GetCurrentByCompanyAndPersonId(int companyId, int personId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var PayStub = _PayStubService.GetCurrentByCompanyIdAndPersonId(companyId, personId);
        return Ok(PayStub);
    }
    [HttpGet("companies/{companyId}/own/count/{count}/skip/{skip}")]
    public async Task<IActionResult> GetCurrentByJobIdAnd(int companyId,int count=6,int skip=0)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, companyId);
        var PayStub =await _PayStubService.GetCurrentOwnPayStubByCompanyId(companyId,count,skip);
        return Ok(PayStub);
    }

    [HttpGet("companies/{companyId}/report")]
    public IActionResult GetByTimeForUser(int companyId, [FromQuery(Name = "startTime")] DateOnly startTime, [FromQuery(Name = "endTime")] DateOnly endTime)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);
        var PayStub = _PayStubService.GetReport(companyId, startTime, endTime);
        return Ok(PayStub);
    }

    [HttpGet("payrolls/{payrollId}")]
    public IActionResult GetCurrentByPayrollId(int payrollId)
    {
        var payStubs = _PayStubService.GetCurrentByPayrollId(payrollId);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, payStubs.FirstOrDefault().Payroll.CompanyId);
        return Ok(payStubs);
    }


    [HttpPost("payrolls/{payrollId}/companies/{companyId}/complete")]
    public IActionResult GeneratePayStubPdfs(int payrollId, int companyId)
    {
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, companyId);

        _PayStubService.GeneratePayStubPdfs(payrollId, companyId);
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _PayStubService.GetById(id);

        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.User, item.Payroll.CompanyId);
        return Ok(item);
    }

    [HttpGet("{id}/document")]
    public IActionResult GetDocumentByPayStubId(int id)
    {
        var item = _PayStubService.GetById(id);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.User, item.Payroll.CompanyId);

        var stream = _PayStubService.GetDocumentByDocumentId(item.DocumentId.Value).Result;
        using (MemoryStream ms = new())
        {
            stream.CopyTo(ms);
            return File(ms.ToArray(), "application/pdf", "mypdf.pdf");
        }       
    }

    [HttpPost()]
    public IActionResult Create([FromBody]PayStubModel paystubModel)
    {       

        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, PayStubModel model)
    {
        var stub = _PayStubService.GetById(id);
        _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, stub.Payroll.CompanyId);

        _PayStubService.Update(id, model);
        return Ok(new { message = "PayStub updated." });
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        _PayStubService.Delete(id);
        return Ok(new { message = "PayStub deleted." });
    }
}