
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SalesMarketing.API.DTOs.MarketingTeam;
using SalesMarketing.API.Enums;
using SalesMarketing.API.Services;

namespace SalesMarketing.API.Controllers
{
    [ApiController]
    [Route("api/marketingteams")]
    [Authorize(Roles = AppRoles.MasterAdmin)]
    public class MarketingTeamsController : ControllerBase
    {
        private readonly MarketingTeamService _service;

        public MarketingTeamsController(MarketingTeamService service)
        {
            _service = service;
        }

        // 1. GET ALL MARKETING TEAMS
        // GET: api/marketingteams
        [HttpGet]
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

        // 2. GET MARKETING TEAM BY ID
        // GET: api/marketingteams/1
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var team = await _service.GetByIdAsync(id);

            if (team == null)
            {
                return NotFound(new
                {
                    message = "Marketing team not found."
                });
            }

            return Ok(team);
        }

        // 3. CREATE MARKETING TEAM
        // POST: api/marketingteams
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] MarketingTeamDto dto)
        {
            try
            {
                var team = await _service.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = team.Id },
                    team);
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
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg &&
                      pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new
                {
                    message = "Team code or active salesman assignment already exists."
                });
            }
        }






        // 4. ADD MEMBER TO EXISTING MARKETING TEAM
        // POST: api/marketingteams/1/members
        [HttpPost("{teamId:long}/members")]
        public async Task<IActionResult> AddMember(
            long teamId,
            [FromBody] MarketingTeamMemberDto dto)
        {
            try
            {
                await _service.AddMemberAsync(teamId, dto);

                return Ok(new
                {
                    message = "Team member added successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
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
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg &&
                      pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new
                {
                    message = "Salesman already has an active team assignment."
                });
            }
        }





        // 5. UPDATE MARKETING TEAM AND MEMBERS
        // PUT: api/marketingteams/1
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] MarketingTeamDto dto)
        {
            try
            {
                bool updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Marketing team not found."
                    });
                }

                return Ok(new
                {
                    message = "Marketing team updated successfully."
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
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg &&
                      pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new
                {
                    message = "Team code or active salesman assignment already exists."
                });
            }
        }

        // 6. ACTIVATE OR DEACTIVATE MARKETING TEAM
        // PATCH: api/marketingteams/1/status?status=2
        //[HttpPatch("{id:long}/status")]
        //public async Task<IActionResult> ChangeStatus(
        //    long id,
        //    [FromQuery] short status)
        //{
        //    try
        //    {
        //        bool updated = await _service.ChangeStatusAsync(
        //            id, status);

        //        if (!updated)
        //        {
        //            return NotFound(new
        //            {
        //                message = "Marketing team not found."
        //            });
        //        }

        //        return Ok(new
        //        {
        //            message = status == 1
        //                ? "Marketing team activated successfully."
        //                : "Marketing team deactivated successfully."
        //        });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new
        //        {
        //            message = ex.Message
        //        });
        //    }
        //    catch (DbUpdateException ex)
        //        when (ex.InnerException is PostgresException pg &&
        //              pg.SqlState == PostgresErrorCodes.UniqueViolation)
        //    {
        //        return Conflict(new
        //        {
        //            message = "A salesman already has an active team assignment."
        //        });
        //    }
        //}







        [HttpPatch("{id:long}/status")]
        public async Task<IActionResult> ChangeStatus(
            long id,
            [FromQuery] short status,
            [FromQuery] long? newLeaderId = null)
        {
            try
            {
                bool updated = await _service.ChangeStatusAsync(
                    id, status, newLeaderId);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Marketing team not found."
                    });
                }

                return Ok(new
                {
                    message = status == 1
                        ? "Marketing team activated successfully."
                        : "Marketing team deactivated successfully."
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
                    message = "Selected salesman already has an active team assignment."
                });
            }
        }


        // 7. DELETE MARKETING TEAM (SOFT DELETE)
        // DELETE: api/marketingteams/1

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                bool deleted = await _service.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message = "Marketing team not found."
                    });
                }

                return Ok(new
                {
                    message = "Marketing team deleted successfully."
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

    }
}
