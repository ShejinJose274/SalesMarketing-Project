
//using Microsoft.EntityFrameworkCore;
//using SalesMarketing.API.Data;
//using SalesMarketing.API.DTOs.MarketingTeam;
//using SalesMarketing.API.DTOs.Common;
//using SalesMarketing.API.Models;

//namespace SalesMarketing.API.Services;

//public class MarketingTeamService
//{
//    private readonly SalesMarketingDbContext _db;

//    public MarketingTeamService(SalesMarketingDbContext db)
//    {
//        _db = db;
//    }

//    // Convert entity to response DTO
//    private static MarketingTeamResponseDto ToDto(Marketingteam team)
//    {
//        return new MarketingTeamResponseDto
//        {
//            Id = team.Id,
//            Teamcode = team.Teamcode,
//            Teamname = team.Teamname,
//            Teamleaderid = team.Teamleaderid,
//            Teamleadername = team.Teamleader.Salesmanname,
//            Description = team.Description,
//            Status = team.Status,
//            Createddate = team.Createddate,
//            Modifieddate = team.Modifieddate,

//            Members = team.Marketingteamdetails
//                .OrderBy(x => x.Id)
//                .Select(x => new MarketingTeamMemberResponseDto
//                {
//                    Salesmanid = x.Salesmanid,
//                    Salesmanname = x.Salesman.Salesmanname,
//                    Role = x.Role,
//                    Targetamount = x.Targetamount,
//                    Status = x.Status
//                })
//                .ToList()
//        };
//    }

//    public async Task<PagedResponseDto<MarketingTeamListItemDto>> GetPagedAsync(
//        string? search, short? status, int page, int pageSize)
//    {
//        page = Math.Max(page, 1);
//        pageSize = Math.Clamp(pageSize, 1, 100);
//        var query = _db.Marketingteams.AsNoTracking();

//        if (!string.IsNullOrWhiteSpace(search))
//        {
//            string value = search.Trim();
//            query = query.Where(x => x.Teamcode.Contains(value) || x.Teamname.Contains(value));
//        }

//        if (status.HasValue)
//            query = query.Where(x => x.Status == status.Value);

//        int total = await query.CountAsync();
//        var items = await query.OrderBy(x => x.Teamcode)
//            .Skip((page - 1) * pageSize)
//            .Take(pageSize)
//            .Select(x => new MarketingTeamListItemDto
//            {
//                Id = x.Id,
//                Teamcode = x.Teamcode,
//                Teamname = x.Teamname,
//                Teamleaderid = x.Teamleaderid,
//                Teamleadername = x.Teamleader.Salesmanname,
//                Salesmancount = x.Marketingteamdetails.Count(d => d.Status == 1),
//                Status = x.Status
//            })
//            .ToListAsync();

//        return new PagedResponseDto<MarketingTeamListItemDto>
//        {
//            Items = items,
//            Page = page,
//            PageSize = pageSize,
//            TotalCount = total
//        };
//    }

//    // GET ALL TEAMS
//    public async Task<List<MarketingTeamResponseDto>> GetAllAsync()
//    {
//        var teams = await _db.Marketingteams
//            .AsNoTracking()
//            .Include(x => x.Teamleader)
//            .Include(x => x.Marketingteamdetails)
//                .ThenInclude(x => x.Salesman)
//            .OrderBy(x => x.Id)
//            .ToListAsync();

//        return teams.Select(ToDto).ToList();
//    }

//    // GET TEAM BY ID
//    public async Task<MarketingTeamResponseDto?> GetByIdAsync(long id)
//    {
//        var team = await _db.Marketingteams
//            .AsNoTracking()
//            .Include(x => x.Teamleader)
//            .Include(x => x.Marketingteamdetails)
//                .ThenInclude(x => x.Salesman)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        return team == null ? null : ToDto(team);
//    }

//    // VALIDATION
//    private async Task ValidateAsync(
//        MarketingTeamDto dto,
//        long? existingTeamId = null)
//    {
//        if (string.IsNullOrWhiteSpace(dto.Teamcode) ||
//            string.IsNullOrWhiteSpace(dto.Teamname))
//        {
//            throw new ArgumentException(
//                "Team code and team name are required.");
//        }

//        if (dto.Status is not 1 and not 2)
//            throw new ArgumentException("Status must be 1 (Active) or 2 (Inactive).");

//        if (dto.Teamcode.Trim().Length > 20 ||
//            dto.Teamname.Trim().Length > 100 ||
//            (dto.Description?.Length ?? 0) > 500)
//        {
//            throw new ArgumentException(
//                "Team code, name or description is too long.");
//        }

//        if (dto.Members == null || dto.Members.Count == 0)
//        {
//            throw new ArgumentException(
//                "A team must contain at least one member.");
//        }

//        if (dto.Members.Any(x =>
//            x.Role < 1 || x.Role > 4 ||
//            x.Targetamount < 0))
//        {
//            throw new ArgumentException(
//                "Invalid member role or target amount.");
//        }

//        var memberIds = dto.Members
//            .Select(x => x.Salesmanid)
//            .ToList();

//        if (memberIds.Distinct().Count() != memberIds.Count)
//        {
//            throw new ArgumentException(
//                "Duplicate salesman in the same team.");
//        }

//        var leaders = dto.Members
//            .Where(x => x.Role == 1)
//            .ToList();

//        if (leaders.Count != 1 ||
//            leaders[0].Salesmanid != dto.Teamleaderid)
//        {
//            throw new ArgumentException(
//                "Exactly one team leader must match Teamleaderid.");
//        }

//        int activeSalesmen = await _db.Salesmen.CountAsync(x =>
//            memberIds.Contains(x.Id) && x.Status == 1);

//        if (activeSalesmen != memberIds.Count)
//        {
//            throw new ArgumentException(
//                "All selected salesmen must be active.");
//        }

//        string code = dto.Teamcode.Trim().ToUpperInvariant();

//        bool codeExists = await _db.Marketingteams.AnyAsync(x =>
//            x.Teamcode == code &&
//            (!existingTeamId.HasValue ||
//             x.Id != existingTeamId.Value));

//        if (codeExists)
//        {
//            throw new InvalidOperationException(
//                "Team code already exists.");
//        }

//        bool assignedElsewhere =
//            await _db.Marketingteamdetails.AnyAsync(x =>
//                memberIds.Contains(x.Salesmanid) &&
//                x.Status == 1 &&
//                (!existingTeamId.HasValue ||
//                 x.Marketingteamid != existingTeamId.Value));

//        if (assignedElsewhere)
//        {
//            throw new InvalidOperationException(
//                "One or more salesmen already belong to another team.");
//        }
//    }

//    // CREATE TEAM
//    public async Task<MarketingTeamResponseDto> CreateAsync(
//        MarketingTeamDto dto)
//    {
//        await ValidateAsync(dto);

//        await using var transaction =
//            await _db.Database.BeginTransactionAsync();

//        var team = new Marketingteam
//        {
//            Teamcode = dto.Teamcode.Trim().ToUpperInvariant(),
//            Teamname = dto.Teamname.Trim(),
//            Teamleaderid = dto.Teamleaderid,
//            Description = dto.Description,
//            Status = dto.Status,
//            Createddate = DateTime.UtcNow
//        };

//        _db.Marketingteams.Add(team);
//        await _db.SaveChangesAsync();

//        foreach (var member in dto.Members)
//        {
//            _db.Marketingteamdetails.Add(new Marketingteamdetail
//            {
//                Marketingteamid = team.Id,
//                Salesmanid = member.Salesmanid,
//                Role = member.Role,
//                Targetamount = member.Targetamount,
//                Status = 1
//            });
//        }

//        await _db.SaveChangesAsync();
//        await transaction.CommitAsync();

//        return (await GetByIdAsync(team.Id))!;
//    }





//    // ADD MEMBER TO EXISTING TEAM
//    public async Task AddMemberAsync(
//        long teamId,
//        MarketingTeamMemberDto dto)
//    {
//        // 1. Check whether the team exists and is active
//        var team = await _db.Marketingteams
//            .FirstOrDefaultAsync(x => x.Id == teamId);

//        if (team == null)
//        {
//            throw new KeyNotFoundException("Marketing team not found.");
//        }

//        if (team.Status != 1)
//        {
//            throw new InvalidOperationException(
//                "Cannot add members to an inactive team.");
//        }

//        // 2. Validate role and target amount
//        if (dto.Role < 1 || dto.Role > 4)
//        {
//            throw new ArgumentException("Invalid member role.");
//        }

//        if (dto.Targetamount < 0)
//        {
//            throw new ArgumentException(
//                "Target amount cannot be negative.");
//        }

//        // Role 1 is reserved for the team's existing leader
//        if (dto.Role == 1 || dto.Salesmanid == team.Teamleaderid)
//        {
//            throw new ArgumentException(
//                "The team leader cannot be added through this endpoint.");
//        }

//        // 3. Check whether the salesman exists and is active
//        var salesman = await _db.Salesmen
//            .FirstOrDefaultAsync(x => x.Id == dto.Salesmanid);

//        if (salesman == null)
//        {
//            throw new ArgumentException("Salesman not found.");
//        }

//        if (salesman.Status != 1)
//        {
//            throw new ArgumentException(
//                "Inactive salesman cannot be assigned to a team.");
//        }

//        // 4. Check whether salesman is active in any team
//        bool alreadyAssigned = await _db.Marketingteamdetails
//            .AnyAsync(x =>
//                x.Salesmanid == dto.Salesmanid &&
//                x.Status == 1);

//        if (alreadyAssigned)
//        {
//            throw new InvalidOperationException(
//                "Salesman is already assigned to an active team.");
//        }

//        // 5. Check whether an inactive record exists in this team
//        var existingMember = await _db.Marketingteamdetails
//            .FirstOrDefaultAsync(x =>
//                x.Marketingteamid == teamId &&
//                x.Salesmanid == dto.Salesmanid);

//        if (existingMember != null)
//        {
//            // Reactivate previous membership
//            existingMember.Role = dto.Role;
//            existingMember.Targetamount = dto.Targetamount;
//            existingMember.Status = 1;
//        }
//        else
//        {
//            // Insert new membership
//            var newMember = new Marketingteamdetail
//            {
//                Marketingteamid = teamId,
//                Salesmanid = dto.Salesmanid,
//                Role = dto.Role,
//                Targetamount = dto.Targetamount,
//                Status = 1
//            };

//            _db.Marketingteamdetails.Add(newMember);
//        }

//        team.Modifieddate = DateTime.UtcNow;

//        await _db.SaveChangesAsync();
//    }










//    // UPDATE TEAM AND MEMBERS
//    public async Task<bool> UpdateAsync(
//        long id,
//        MarketingTeamDto dto)
//    {
//        var team = await _db.Marketingteams
//            .Include(x => x.Marketingteamdetails)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (team == null)
//            return false;

//        await ValidateAsync(dto, id);

//        await using var transaction =
//            await _db.Database.BeginTransactionAsync();

//        team.Teamcode = dto.Teamcode.Trim().ToUpperInvariant();
//        team.Teamname = dto.Teamname.Trim();
//        team.Teamleaderid = dto.Teamleaderid;
//        team.Description = dto.Description;
//        team.Status = dto.Status;
//        team.Modifieddate = DateTime.UtcNow;

//        var selectedIds = dto.Members
//            .Select(x => x.Salesmanid)
//            .ToList();

//        var removedMembers = team.Marketingteamdetails
//            .Where(x => !selectedIds.Contains(x.Salesmanid))
//            .ToList();

//        _db.Marketingteamdetails.RemoveRange(removedMembers);

//        foreach (var oldMember in team.Marketingteamdetails.Except(removedMembers))
//        {
//            oldMember.Status = dto.Status == 1 ? (short)1 : (short)2;
//        }

//        foreach (var member in dto.Members)
//        {
//            var existingMember = team.Marketingteamdetails
//                .FirstOrDefault(x => x.Salesmanid == member.Salesmanid);

//            if (existingMember != null)
//            {
//                // Existing member or returning member
//                existingMember.Role = member.Role;
//                existingMember.Targetamount = member.Targetamount;
//                existingMember.Status = 1;
//            }
//            else
//            {
//                // New member
//                _db.Marketingteamdetails.Add(new Marketingteamdetail
//                {
//                    Marketingteamid = team.Id,
//                    Salesmanid = member.Salesmanid,
//                    Role = member.Role,
//                    Targetamount = member.Targetamount,
//                    Status = dto.Status == 1 ? (short)1 : (short)2
//                });
//            }
//        }

//        await _db.SaveChangesAsync();
//        await transaction.CommitAsync();

//        return true;
//    }

//    // ACTIVATE OR DEACTIVATE TEAM
//    public async Task<bool> ChangeStatusAsync(long id, short status)
//    {
//        if (status != 1 && status != 2)
//        {
//            throw new ArgumentException(
//                "Status must be 1 (Active) or 2 (Inactive).");
//        }

//        var team = await _db.Marketingteams
//            .Include(x => x.Marketingteamdetails)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (team == null)
//            return false;

//        if (status == 1)
//        {
//            // Team must have a valid active leader
//            var leader = team.Marketingteamdetails
//                .FirstOrDefault(x =>
//                    x.Salesmanid == team.Teamleaderid &&
//                    x.Role == 1);

//            if (leader == null)
//            {
//                throw new InvalidOperationException(
//                    "Team must have a valid leader.");
//            }

//            bool available = !await _db.Marketingteamdetails
//                .AnyAsync(x =>
//                    x.Salesmanid == team.Teamleaderid &&
//                    x.Marketingteamid != id &&
//                    x.Status == 1);

//            bool leaderActive = await _db.Salesmen
//                .AnyAsync(x =>
//                    x.Id == team.Teamleaderid &&
//                    x.Status == 1);

//            if (!available || !leaderActive)
//            {
//                throw new InvalidOperationException(
//                    "Team leader is not available.");
//            }

//            // Only the leader is reactivated automatically.
//            foreach (var member in team.Marketingteamdetails)
//            {
//                member.Status =
//                    member.Salesmanid == team.Teamleaderid
//                        ? (short)1
//                        : (short)2;
//            }
//        }
//        else
//        {
//            // Deactivate all members
//            foreach (var member in team.Marketingteamdetails)
//            {
//                member.Status = 2;
//            }
//        }

//        team.Status = status;
//        team.Modifieddate = DateTime.UtcNow;

//        await _db.SaveChangesAsync();

//        return true;
//    }

//    public async Task<bool> DeleteAsync(long id)
//    {
//        var team = await _db.Marketingteams.FirstOrDefaultAsync(x => x.Id == id);
//        if (team == null)
//            return false;

//        _db.Marketingteams.Remove(team);
//        await _db.SaveChangesAsync();
//        return true;
//    }
//}



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
        var query = _db.Marketingteams.AsNoTracking();

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
        var teams = await _db.Marketingteams
            .AsNoTracking()
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
            .FirstOrDefaultAsync(x => x.Id == id);

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
            Description = dto.Description,
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
                Status = 1
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
            return false;

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

        team.Teamcode = dto.Teamcode.Trim().ToUpperInvariant();
        team.Teamname = dto.Teamname.Trim();
        team.Teamleaderid = dto.Teamleaderid;
        team.Description = dto.Description;
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

    // ACTIVATE OR DEACTIVATE TEAM
    public async Task<bool> ChangeStatusAsync(long id, short status)
    {
        if (status != 1 && status != 2)
        {
            throw new ArgumentException(
                "Status must be 1 (Active) or 2 (Inactive).");
        }

        var team = await _db.Marketingteams
            .Include(x => x.Marketingteamdetails)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (team == null)
            return false;

        if (status == 1)
        {
            // Team must have a valid active leader
            var leader = team.Marketingteamdetails
                .FirstOrDefault(x =>
                    x.Salesmanid == team.Teamleaderid &&
                    x.Role == 1);

            if (leader == null)
            {
                throw new InvalidOperationException(
                    "Team must have a valid leader.");
            }

            bool available = !await _db.Marketingteamdetails
                .AnyAsync(x =>
                    x.Salesmanid == team.Teamleaderid &&
                    x.Marketingteamid != id &&
                    x.Status == 1);

            bool leaderActive = await _db.Salesmen
                .AnyAsync(x =>
                    x.Id == team.Teamleaderid &&
                    x.Status == 1);

            if (!available || !leaderActive)
            {
                throw new InvalidOperationException(
                    "Team leader is not available.");
            }

            // Only the leader is reactivated automatically.
            foreach (var member in team.Marketingteamdetails)
            {
                member.Status =
                    member.Salesmanid == team.Teamleaderid
                        ? (short)1
                        : (short)2;
            }
        }
        else
        {
            // Deactivate all members
            foreach (var member in team.Marketingteamdetails)
            {
                member.Status = 2;
            }
        }

        team.Status = status;
        team.Modifieddate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var team = await _db.Marketingteams.FirstOrDefaultAsync(x => x.Id == id);
        if (team == null)
            return false;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var assignments = await _db.Marketingteamdetails
            .Where(x => x.Marketingteamid == id).ToListAsync();
        _db.Marketingteamdetails.RemoveRange(assignments);
        await _db.SaveChangesAsync();
        _db.Marketingteams.Remove(team);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
