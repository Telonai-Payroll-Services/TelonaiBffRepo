using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Models.IRS;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IRS941Controller : ControllerBase
    {
        private readonly IFormNineFortyOneService _irsService;
        public IRS941Controller(IFormNineFortyOneService irsService)
        {
            _irsService = irsService;
        }

        [HttpGet]
        public ActionResult<List<FormNineFortyOneModel>> GenerateXML()
        {
            var irs941Data  = _irsService.GetCurrent941FormsAsync();
            return Ok(irs941Data);
        }

    }
}
