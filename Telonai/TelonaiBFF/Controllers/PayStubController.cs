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

    [HttpGet("companies/{companies}/persons/{personId}")]
    public IActionResult GetCurrentByJobIdAndPersonId(int companyId, int personId)
    {
        _scopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.User, companyId);
        var PayStub = _PayStubService.GetCurrentByCompanyIdAndPersonId(companyId, personId);
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


    [HttpGet("payrolls/{payrollId}/companies/{companyId}/generate")]
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
        if (stub != null)
        {
            _scopedAuthorization.ValidateByCompanyId(Request.HttpContext.User, AuthorizationType.Admin, stub.Payroll.CompanyId);
            _PayStubService.Update(id, model);
            return Ok(new { message = "PayStub updated." });
        }
        else
        {
            return NotFound();
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(int id)
    {
        if(_PayStubService.Delete(id))
        {
            return Ok(new { message = "PayStub deleted." });
        }
        else
        {
            return NotFound();
        }
    }
}