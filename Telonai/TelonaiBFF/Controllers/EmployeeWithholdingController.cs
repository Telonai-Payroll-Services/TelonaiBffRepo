namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class EmployeeWithholdingController : ControllerBase
{
    private readonly IEmployeeWithholdingService<EmployeeWithholdingModel,EmployeeWithholding> _empWithholdingservice;
    public EmployeeWithholdingController(IEmployeeWithholdingService<EmployeeWithholdingModel, EmployeeWithholding> empWithholdingservice)
    {
        _empWithholdingservice = empWithholdingservice;
    }

    [HttpGet("/{id}/details")]
    public IActionResult GetById(int id)
    {
        var result = _empWithholdingservice.GetById(id);
        return Ok(result);
    }

    [HttpPost()]
    public IActionResult Create([FromForm] IFormFile file, [FromBody] EmployeeWithholdingModel model)
    {

        using (Stream stream = new MemoryStream())
        {
            file.CopyTo(stream);
            _empWithholdingservice.CreateAsync(model,stream);

        }
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, EmployeeWithholdingModel model)
    {
        _empWithholdingservice.UpdateAsync(id, model);
        return Ok(new { message = "Item updated." });
    }


    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _empWithholdingservice.DeleteAsync(id);
        return Ok(new { message = "Item deleted." });
    }
}