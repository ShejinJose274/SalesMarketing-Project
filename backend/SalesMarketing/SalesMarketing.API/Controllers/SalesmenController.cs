
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SalesMarketing.API.DTOs.Salesman;
using SalesMarketing.API.Enums;
using SalesMarketing.API.Services;

namespace SalesMarketing.API.Controllers;

[ApiController]
[Route("api/salesmen")]
//[Authorize]
public class SalesmenController : ControllerBase
{
    private readonly SalesmanService _service;

    public SalesmenController(SalesmanService service)
    {
        _service = service;
    }

    // GET: api/salesmen
    [HttpGet]
    //[Authorize(Roles =
    //    AppRoles.MasterAdmin + "," + AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] short? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (status is not null and not 1 and not 2)
            return BadRequest(new { message = "Status must be 1 (Active) or 2 (Inactive)." });

        return Ok(await _service.GetPagedAsync(search, status, page, pageSize));
    }

    // GET: api/salesmen/1
    [HttpGet("{id:long}")]
    //[Authorize(Roles =
    //    AppRoles.MasterAdmin + "," + AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> GetById(long id)
    {
        var salesman = await _service.GetByIdAsync(id);

        if (salesman == null)
            return NotFound(new
            {
                message = "Salesman not found."
            });

        return Ok(salesman);
    }

    // POST: api/salesmen
    [HttpPost]
    //[Authorize(Roles = AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> Create(SalesmanDto dto)
    {
        try
        {
            var salesman = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = salesman.Id },
                salesman);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new
            {
                message = "Salesman code already exists."
            });
        }
    }

    // PUT: api/salesmen/1
    [HttpPut("{id:long}")]
    //[Authorize(Roles = AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> Update(
        long id,
        SalesmanDto dto)
    {
        try
        {
            bool updated = await _service.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new
                {
                    message = "Salesman not found."
                });

            return Ok(new
            {
                message = "Salesman updated successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new
            {
                message = "Salesman code already exists."
            });
        }
    }

    // PATCH: api/salesmen/1/status

    [HttpPatch("{id:long}/status")]
    //[Authorize(Roles = AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> ChangeStatus(
        long id,
        [FromQuery] short status)
    {
        try
        {
            bool updated = await _service.ChangeStatusAsync(id, status);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Salesman not found."
                });
            }

            return Ok(new
            {
                message = status == 1
                    ? "Salesman activated successfully."
                    : "Salesman deactivated successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }


    // DELETE: api/salesmen/1
    [HttpDelete("{id:long}")]
    [Authorize(Roles = AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    message = "Salesman not found."
                });

            return Ok(new
            {
                message = "Salesman deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Conflict(new
            {
                message = "Cannot delete a salesman assigned to a marketing team."
            });
        }
    }



    // GET: api/salesmen/status/1

    [HttpGet("status/{status:int}")]
    //[Authorize(Roles =
    //    AppRoles.MasterAdmin + "," + AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> GetByStatus(int status)
    {
        if (status != 1 && status != 2)
        {
            return BadRequest(new
            {
                message = "Status must be 1 (Active) or 2 (Inactive)."
            });
        }

        var salesmen = await _service.GetByStatusAsync((short)status);

        return Ok(salesmen);
    }



    // GET: api/salesmen/available
    [HttpGet("available")]
    //[Authorize(Roles =
    //    AppRoles.MasterAdmin + "," + AppRoles.SalesManagerAdmin)]
    public async Task<IActionResult> GetAvailableSalesmen()
    {
        var salesmen = await _service.GetAvailableSalesmenAsync();

        return Ok(salesmen);
    }


}
