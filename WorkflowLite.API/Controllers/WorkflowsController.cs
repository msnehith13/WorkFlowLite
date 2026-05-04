using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowLite.Application.DTOs;
using WorkflowLite.Application.Services;

namespace WorkflowLite.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly WorkflowService _workflows;
    public WorkflowsController(WorkflowService workflows) => _workflows = workflows;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Get all workflows for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkflowDto>), 200)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _workflows.GetAllAsync(UserId));

    /// <summary>Get a single workflow by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _workflows.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    /// <summary>Create a new workflow with steps and rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkflowDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowRequest req)
    {
        var result = await _workflows.CreateAsync(req, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update workflow name, description, and active state.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkflowRequest req)
    {
        try { return Ok(await _workflows.UpdateAsync(id, req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    /// <summary>Delete a workflow and all its steps.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _workflows.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Trigger a workflow run. Pass context as JSON key-value pairs for rule evaluation.</summary>
    [HttpPost("{id:guid}/trigger")]
    [ProducesResponseType(typeof(WorkflowInstanceDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Trigger(Guid id, [FromBody] TriggerWorkflowRequest req)
    {
        try { return Ok(await _workflows.TriggerAsync(id, UserId, req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}