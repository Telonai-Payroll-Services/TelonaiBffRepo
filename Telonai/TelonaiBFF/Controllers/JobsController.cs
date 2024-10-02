namespace TelonaiWebApi.Controllers;

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
public class JobsController : ControllerBase
{
    private readonly IJobService<JobModel,Job> _service;
    private readonly IMapper _mapper;

    public JobsController(IJobService<JobModel, Job> jobService, IMapper mapper)
    {
        _service = jobService;
        _mapper = mapper;

    }

    [HttpGet]
    public IActionResult GetAll()
    {
        ScopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        var jobs = _service.Get();
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var job = _service.GetById(id);
        return Ok(job);
    }

    [HttpPost]
    public IActionResult Create(JobRequestModel model)
    {
        var principal = Request.HttpContext.User;
        ScopedAuthorization.ValidateByCompanyId(principal, AuthorizationType.Admin, model.CompanyId);

        var jModel = _mapper.Map<JobModel>(model);
        var email = principal.Claims.First(e => e.Type == "email").Value;
        var username = principal.Claims.First(e => e.Type == "username").Value;

        _service.CreateAsync(jModel,username,email);
        return Ok(new { message = "Job created" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, JobModel model)
    {
        _service.UpdateAsync(id, model);
        ScopedAuthorization.ValidateByJobId(Request.HttpContext.User, AuthorizationType.Admin, id);
        return Ok(new { message = "Job updated" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        ScopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        _service.DeleteAsync(id);
        return Ok(new { message = "Job deleted" });
    }
}