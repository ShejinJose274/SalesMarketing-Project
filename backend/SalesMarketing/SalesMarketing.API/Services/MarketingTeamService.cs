
using SalesMarketing.API.Enums;
using Microsoft.EntityFrameworkCore;
using SalesMarketing.API.Data;
using SalesMarketing.API.DTOs.MarketingTeam;
using SalesMarketing.API.DTOs.Common;
using SalesMarketing.API.Models;

namespace SalesMarketing.API.Services;

public class MarketingTeamService
{
    private readonly SalesMarketingDbContext _db;

    public MarketingTeamService(SalesMarketingDbContext db)
    {
        _db = db;
    }

    // Convert entity to response DTO
    private static MarketingTeamResponseDto ToDto(Marketingteam team)
    {
        return new MarketingTeamResponseDto
        {
            Id = team.Id,
            Teamcode = team.Teamcode,
            Teamname = team.Teamname,
            Teamleaderid = team.Teamleaderid,
            Teamleadername = team.Teamleader.Salesmanname,
            Description = team.Description,
            Status = team.Status,
            Createddate = team.Createddate,
            Modifieddate = team.Modifieddate,

            Members = team.Marketingteamdetails
                .OrderBy(x => x.Id)
                .Select(x => new MarketingTeamMemberResponseDto
                {
                    Salesmanid = x.Salesmanid,
                    Salesmanname = x.Salesman.Salesmanname,
                    Role = x.Role,
                    Targetamount = x.Targetamount,
                    Status = x.Status
                })
                .ToList()
        };
    }

    public async Task<PagedResponseDto<MarketingTeamListItemDto>> GetPagedAsync(
        string? search, short? status, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Marketingteams
            .AsNoTracking()
            .Where(x => x.Status != (short)MarketingTeamStatus.Deleted);


        if (!string.IsNullOrWhiteSpace(search))
        {
            string value = search.Trim();
            string escaped = value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            string pattern = $"%{escaped}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Teamcode, pattern, "\\") ||
                EF.Functions.ILike(x.Teamname, pattern, "\\"));
        }

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        int total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Teamcode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MarketingTeamListItemDto
            {
                Id = x.Id,
                Teamcode = x.Teamcode,
                Teamname = x.Teamname,
                Teamleaderid = x.Teamleaderid,
                Teamleadername = x.Teamleader.Salesmanname,
                Salesmancount = x.Marketingteamdetails.Count(d => d.Status == 1),
                Status = x.Status
            })
            .ToListAsync();

        return new PagedResponseDto<MarketingTeamListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    // GET ALL TEAMS
    public async Task<List<MarketingTeamResponseDto>> GetAllAsync()
    {
        var teams = await _db.Marketingteams.AsNoTracking().Where(x => x.Status != (short)MarketingTeamStatus.Deleted)
            .Include(x => x.Teamleader)
            .Include(x => x.Marketingteamdetails)
                .ThenInclude(x => x.Salesman)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return teams.Select(ToDto).ToList();
    }

    // GET TEAM BY ID
    public async Task<MarketingTeamResponseDto?> GetByIdAsync(long id)
    {
        var team = await _db.Marketingteams
            .AsNoTracking()
            .Include(x => x.Teamleader)
            .Include(x => x.Marketingteamdetails)
                .ThenInclude(x => x.Salesman)
            .FirstOrDefaultAsync(x =>x.Id == id && x.Status != (short)MarketingTeamStatus.Deleted); 

        return team == null ? null : ToDto(team);
    }

    // VALIDATION
    private async Task ValidateAsync(
        MarketingTeamDto dto,
        long? existingTeamId = null)
    {
        if (string.IsNullOrWhiteSpace(dto.Teamcode) ||
            string.IsNullOrWhiteSpace(dto.Teamname))
        {
            throw new ArgumentException(
                "Team code and team name are required.");
        }

        if (dto.Status is not 1 and not 2)
            throw new ArgumentException("Status must be 1 (Active) or 2 (Inactive).");

        if (dto.Teamcode.Trim().Length > 20 ||
            dto.Teamname.Trim().Length > 100 ||
            (dto.Description?.Length ?? 0) > 500)
        {
            throw new ArgumentException(
                "Team code, name or description is too long.");
        }

        if (dto.Members != null && dto.Members.Count > 5)
            throw new ArgumentException("A marketing team can have a maximum of 5 members, including the team leader.");

        if (dto.Members == null || dto.Members.Count == 0)
        {
            throw new ArgumentException(
                "A team must contain at least one member.");
        }

        if (dto.Members.Any(x =>
            x.Role < 1 || x.Role > 4 ||
            x.Targetamount < 0))
        {
            throw new ArgumentException(
                "Invalid member role or target amount.");
        }

        var memberIds = dto.Members
            .Select(x => x.Salesmanid)
            .ToList();

        if (memberIds.Distinct().Count() != memberIds.Count)
        {
            throw new ArgumentException(
                "Duplicate salesman in the same team.");
        }

        var leaders = dto.Members
            .Where(x => x.Role == 1)
            .ToList();

        if (leaders.Count != 1 ||
            leaders[0].Salesmanid != dto.Teamleaderid)
        {
            throw new ArgumentException(
                "Exactly one team leader must match Teamleaderid.");
        }

        int activeSalesmen = await _db.Salesmen.CountAsync(x =>
            memberIds.Contains(x.Id) && x.Status == 1);

        if (activeSalesmen != memberIds.Count)
        {
            throw new ArgumentException(
                "All selected salesmen must be active.");
        }

        string code = dto.Teamcode.Trim().ToUpperInvariant();

        bool codeExists = await _db.Marketingteams.AnyAsync(x =>
            x.Teamcode == code &&
            (!existingTeamId.HasValue ||
             x.Id != existingTeamId.Value));

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Team code already exists.");
        }

        bool assignedElsewhere =
            await _db.Marketingteamdetails.AnyAsync(x =>
                memberIds.Contains(x.Salesmanid) &&
                x.Status == 1 &&
                (!existingTeamId.HasValue ||
                 x.Marketingteamid != existingTeamId.Value));

        if (assignedElsewhere)
        {
            throw new InvalidOperationException(
                "One or more salesmen already belong to another team.");
        }
    }

    // CREATE TEAM
    public async Task<MarketingTeamResponseDto> CreateAsync(
        MarketingTeamDto dto)
    {
        await ValidateAsync(dto);

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var team = new Marketingteam
        {
            Teamcode = dto.Teamcode.Trim().ToUpperInvariant(),
            Teamname = dto.Teamname.Trim(),
            Teamleaderid = dto.Teamleaderid,
            Description = NormalizeOptional(dto.Description),
            Status = dto.Status,
            Createddate = DateTime.UtcNow
        };

        _db.Marketingteams.Add(team);
        await _db.SaveChangesAsync();

        foreach (var member in dto.Members)
        {
            _db.Marketingteamdetails.Add(new Marketingteamdetail
            {
                Marketingteamid = team.Id,
                Salesmanid = member.Salesmanid,
                Role = member.Role,
                Targetamount = member.Targetamount,
                //Status = 1
                Status = dto.Status == 1 ? (short)1 : (short)2
            });
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return (await GetByIdAsync(team.Id))!;
    }





    // ADD MEMBER TO EXISTING TEAM
    public async Task AddMemberAsync(
        long teamId,
        MarketingTeamMemberDto dto)
    {
        // 1. Check whether the team exists and is active
        var team = await _db.Marketingteams
            .FirstOrDefaultAsync(x => x.Id == teamId);

        if (team == null)
        {
            throw new KeyNotFoundException("Marketing team not found.");
        }

        if (team.Status != 1)
        {
            throw new InvalidOperationException(
                "Cannot add members to an inactive team.");
        }

        int currentMemberCount = await _db.Marketingteamdetails.CountAsync(x =>
            x.Marketingteamid == teamId && x.Status == 1);
        if (currentMemberCount >= 5)
            throw new InvalidOperationException("A marketing team can have a maximum of 5 active members.");

        // 2. Validate role and target amount
        if (dto.Role < 1 || dto.Role > 4)
        {
            throw new ArgumentException("Invalid member role.");
        }

        if (dto.Targetamount < 0)
        {
            throw new ArgumentException(
                "Target amount cannot be negative.");
        }

        // Role 1 is reserved for the team's existing leader
        if (dto.Role == 1 || dto.Salesmanid == team.Teamleaderid)
        {
            throw new ArgumentException(
                "The team leader cannot be added through this endpoint.");
        }

        // 3. Check whether the salesman exists and is active
        var salesman = await _db.Salesmen
            .FirstOrDefaultAsync(x => x.Id == dto.Salesmanid);

        if (salesman == null)
        {
            throw new ArgumentException("Salesman not found.");
        }

        if (salesman.Status != 1)
        {
            throw new ArgumentException(
                "Inactive salesman cannot be assigned to a team.");
        }

        // 4. Check whether salesman is active in any team
        bool alreadyAssigned = await _db.Marketingteamdetails
            .AnyAsync(x =>
                x.Salesmanid == dto.Salesmanid &&
                x.Status == 1);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException(
                "Salesman is already assigned to an active team.");
        }

        // 5. Check whether an inactive record exists in this team
        var existingMember = await _db.Marketingteamdetails
            .FirstOrDefaultAsync(x =>
                x.Marketingteamid == teamId &&
                x.Salesmanid == dto.Salesmanid);

        if (existingMember != null)
        {
            // Reactivate previous membership
            existingMember.Role = dto.Role;
            existingMember.Targetamount = dto.Targetamount;
            existingMember.Status = 1;
        }
        else
        {
            // Insert new membership
            var newMember = new Marketingteamdetail
            {
                Marketingteamid = teamId,
                Salesmanid = dto.Salesmanid,
                Role = dto.Role,
                Targetamount = dto.Targetamount,
                Status = 1
            };

            _db.Marketingteamdetails.Add(newMember);
        }

        team.Modifieddate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }










    // UPDATE TEAM AND MEMBERS
    public async Task<bool> UpdateAsync(
        long id,
        MarketingTeamDto dto)
    {
        var team = await _db.Marketingteams
            .Include(x => x.Marketingteamdetails)
            .FirstOrDefaultAsync(x => x.Id == id);


        if (team == null)
        {
            return false;
        }

        if (team.Status == (short)MarketingTeamStatus.Deleted)
        {
            throw new InvalidOperationException(
                "Cannot edit a deleted marketing team.");
        }


        // An inactive team cannot gain new assignments through the PUT endpoint.
        if (team.Status == 2)
        {
            var existingIds = team.Marketingteamdetails.Select(x => x.Salesmanid).ToHashSet();
            if (dto.Members.Any(x => !existingIds.Contains(x.Salesmanid)))
                throw new InvalidOperationException("Cannot add members to an inactive team. Activate it first.");
            if (dto.Status != 2)
                throw new InvalidOperationException("Activate the team using the status endpoint before editing membership.");
        }

        await ValidateAsync(dto, id);

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var requestedCode = dto.Teamcode.Trim().ToUpperInvariant();
        if (!string.Equals(requestedCode, team.Teamcode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Team code cannot be changed after creation.");
        }
        team.Teamname = dto.Teamname.Trim();
        team.Teamleaderid = dto.Teamleaderid;
        team.Description = NormalizeOptional(dto.Description);
        team.Status = dto.Status;
        team.Modifieddate = DateTime.UtcNow;

        var selectedIds = dto.Members
            .Select(x => x.Salesmanid)
            .ToList();

        var removedMembers = team.Marketingteamdetails
            .Where(x => !selectedIds.Contains(x.Salesmanid))
            .ToList();

        // Preserve assignment history rather than physically deleting it.
        foreach (var removed in removedMembers)
            removed.Status = 2;

        foreach (var oldMember in team.Marketingteamdetails.Except(removedMembers))
        {
            oldMember.Status = dto.Status == 1 ? (short)1 : (short)2;
        }

        foreach (var member in dto.Members)
        {
            var existingMember = team.Marketingteamdetails
                .FirstOrDefault(x => x.Salesmanid == member.Salesmanid);

            if (existingMember != null)
            {
                // Existing member or returning member
                existingMember.Role = member.Role;
                existingMember.Targetamount = member.Targetamount;
                existingMember.Status = dto.Status == 1 ? (short)1 : (short)2;
            }
            else
            {
                // New member
                _db.Marketingteamdetails.Add(new Marketingteamdetail
                {
                    Marketingteamid = team.Id,
                    Salesmanid = member.Salesmanid,
                    Role = member.Role,
                    Targetamount = member.Targetamount,
                    Status = dto.Status == 1 ? (short)1 : (short)2
                });
            }
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    public async Task<bool> ChangeStatusAsync(
        long id,
        short status,
        long? newLeaderId = null)
    {
        if (status != 1 && status != 2)
        {
            throw new ArgumentException(
                "Status must be 1 (Active) or 2 (Inactive).");
        }

        if (status == 2 && newLeaderId.HasValue)
        {
            throw new ArgumentException(
                "Replacement leader is only allowed during activation.");
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        // Serialize team assignment changes with other operations
        // that use the same PostgreSQL advisory lock.
        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(7142026)");

        var team = await _db.Marketingteams
            .Include(x => x.Marketingteamdetails)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (team == null ||
            team.Status == (short)MarketingTeamStatus.Deleted)
        {
            return false;
        }

        if (status == 1)
        {
            if (team.Status == 1)
            {
                throw new InvalidOperationException(
                    "Marketing team is already active.");
            }

            long selectedLeaderId =
                newLeaderId ?? team.Teamleaderid;

            // The selected salesman must exist and be active.
            bool salesmanActive = await _db.Salesmen
                .AnyAsync(x =>
                    x.Id == selectedLeaderId &&
                    x.Status == 1);

            if (!salesmanActive)
            {
                throw new InvalidOperationException(
                    "Selected team leader is not an active salesman.");
            }

            // Check whether the salesman belongs to another
            // active team.
            bool assignedElsewhere =
                await _db.Marketingteamdetails.AnyAsync(x =>
                    x.Salesmanid == selectedLeaderId &&
                    x.Marketingteamid != id &&
                    x.Status == 1);

            if (assignedElsewhere)
            {
                if (!newLeaderId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Current Team Leader is unavailable. " +
                        "Select another available salesman to activate this team.");
                }

                throw new InvalidOperationException(
                    "Selected salesman already belongs to another active team.");
            }

            // Find the selected leader's historical membership.
            var selectedMember = team.Marketingteamdetails
                .FirstOrDefault(x =>
                    x.Salesmanid == selectedLeaderId);

            // New leader can be added, subject to the team limit.
            if (selectedMember == null)
            {
                if (team.Marketingteamdetails.Count >= 5)
                {
                    throw new InvalidOperationException(
                        "Team already has 5 historical member records. " +
                        "Choose an existing available member as leader.");
                }

                selectedMember = new Marketingteamdetail
                {
                    Marketingteamid = team.Id,
                    Salesmanid = selectedLeaderId,
                    Role = 1,
                    Targetamount = 0,
                    Status = 1
                };

                _db.Marketingteamdetails.Add(selectedMember);
            }

            // Keep all historical assignments inactive.
            // Ensure only the selected leader has role 1.
            foreach (var member in team.Marketingteamdetails)
            {
                member.Status = 2;

                if (member.Role == 1)
                {
                    member.Role = 2;
                }
            }

            selectedMember.Role = 1;
            selectedMember.Status = 1;

            team.Teamleaderid = selectedLeaderId;
            team.Status = 1;
        }
        else
        {
            // Deactivate team and release all its salesmen.
            foreach (var member in team.Marketingteamdetails)
            {
                member.Status = 2;
            }

            team.Status = 2;
        }

        team.Modifieddate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }




    /// DELETE TEAM
    public async Task<bool> DeleteAsync(long id)
    {
        var team = await _db.Marketingteams
            .FirstOrDefaultAsync(x => x.Id == id);

        if (team == null ||
            team.Status == (short)MarketingTeamStatus.Deleted)
        {
            return false;
        }

        // Active teams must be deactivated first.
        if (team.Status == (short)MarketingTeamStatus.Active)
        {
            throw new InvalidOperationException(
                "Deactivate team first to delete");
        }

        // Only inactive teams can be soft deleted.
        team.Status = (short)MarketingTeamStatus.Deleted;
        team.Modifieddate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

}
