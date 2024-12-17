using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using Amazon.SimpleEmail.Model;
using System.Buffers;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class MobileAppVersionController : ControllerBase
{
    private readonly IMobileAppVersionService _mobileAppVersionService;
    private readonly IScopedAuthorization _scopedAuthorization;

    public MobileAppVersionController(IMobileAppVersionService mobileAppVersionService, IScopedAuthorization scopedAuthorization)
    {
        _mobileAppVersionService = mobileAppVersionService;
        _scopedAuthorization = scopedAuthorization;
    }

    [HttpGet]
    [Route("{platform}")]
    public async Task<ActionResult<MobileAppVersionModel>> GetLatestAppVersion(int platform)
    {
        var version = await _mobileAppVersionService.GetLatestAppVersion(platform);
        if (version == null) return NotFound();

        //TODO: Add the links to google store and appstore
        version.ApplePath = "";
        version.GooglePath = "";

        return Ok(version);
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpGet]
    [Route("All")]
    public IActionResult GetAll()
    {
        var versions = _mobileAppVersionService.GetAll();
        return Ok(versions.Result);
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpPost]
    public async Task<ActionResult<MobileAppVersionModel>> Create(MobileAppVersionModel mobileAppVersion)
    {
        if (mobileAppVersion == null)
        {
            return BadRequest();
        }

        await _mobileAppVersionService.CreateAsync(mobileAppVersion);

        return Ok();
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MobileAppVersionModel mobileAppVersion)
    {
        await _mobileAppVersionService.UpdateAsync(id, mobileAppVersion);

        return NoContent();
    }

    [Authorize(Policy = "SystemAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mobileAppVersionService.DeleteAsync(id);

        return NoContent();
    }

}
