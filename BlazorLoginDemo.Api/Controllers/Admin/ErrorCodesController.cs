using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLoginDemo.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/errorcodes")]
public sealed class ErrorCodesController : ControllerBase
{
    private readonly IErrorCodeService _service;

    public ErrorCodesController(IErrorCodeService service)
    {
        _service = service;
    }

    // GET: api/v1/admin/errorcodes
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ErrorCodeUnified>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    // GET: api/v1/admin/errorcodes/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ErrorCodeUnified>> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // GET: api/v1/admin/errorcodes/by-code/{errorCode}
    [HttpGet("by-code/{errorCode}")]
    public async Task<ActionResult<ErrorCodeUnified>> GetByCode(string errorCode)
    {
        var result = await _service.GetErrorAsync(errorCode);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // POST: api/v1/admin/errorcodes
    [HttpPost]
    public async Task<ActionResult<ErrorCodeUnified>> Create(ErrorCodeUnified entity)
    {
        var created = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/v1/admin/errorcodes/{id}
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, ErrorCodeUnified entity)
    {
        if (id != entity.Id)
            return BadRequest("ID mismatch");

        var updated = await _service.UpdateAsync(entity);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/v1/admin/errorcodes/{id}
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
