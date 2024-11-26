using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Controllers;


[ApiController]
[Route("[controller]")]
[Authorize()]
public class AgentController : ControllerBase
{
    private readonly IAgentService _service;

    public AgentController(IAgentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<AgentFieldValue>> Create(AgentFieldValueModel model)
    {
        var result = await _service.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet] public async Task<IActionResult> Get() 
    { 
        var result = await _service.Get();
        return Ok(result); 
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetById(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AgentFieldValueModel model)
    {
        await _service.UpdateAsync(id, model);
        return NoContent();
    }
}


