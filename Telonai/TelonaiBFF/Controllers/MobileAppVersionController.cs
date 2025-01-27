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
        version.AppPath = platform == 1 ? "https://play.google.com/store/apps/details?id=com.telonai.app" : "https://apps.apple.com/us/app/telonai/id6738379955";

        return Ok(version);
    }

    [Authorize]
    [HttpGet]
    [Route("All")]
    public IActionResult GetAll()
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var versions = _mobileAppVersionService.GetAll();
        return Ok(versions.Result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MobileAppVersionModel>> Create(MobileAppVersionModel mobileAppVersion)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        if (mobileAppVersion == null)
        {
            return BadRequest();
        }

        await _mobileAppVersionService.CreateAsync(mobileAppVersion);

        return Ok();
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MobileAppVersionModel mobileAppVersion)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        await _mobileAppVersionService.UpdateAsync(id, mobileAppVersion);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        await _mobileAppVersionService.DeleteAsync(id);

        return NoContent();
    }

}
