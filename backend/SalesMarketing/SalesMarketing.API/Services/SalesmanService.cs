
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SalesMarketing.API.Data;
using SalesMarketing.API.DTOs.Salesman;
using SalesMarketing.API.DTOs.Common;
using SalesMarketing.API.Models;

namespace SalesMarketing.API.Services;

public class SalesmanService
{
    private readonly SalesMarketingDbContext _context;

    public SalesmanService(SalesMarketingDbContext context)     //DI
    {
        _context = context;
    }








    public async Task<PagedResponseDto<SalesmanResponseDto>> GetPagedAsync(
        string? search,
        short? status,
        int page,
        int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Salesmen.AsNoTracking();

        // Case-insensitive search
        if (!string.IsNullOrWhiteSpace(search))
        {
            string value = search.Trim();

            // Escape LIKE wildcard characters
            string escapedValue = value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            string pattern = $"%{escapedValue}%";

            query = query.Where(x =>                            //ILIKE is used for case-insensitive search in PostgreSQL
                EF.Functions.ILike(x.Salesmancode, pattern, "\\") ||
                EF.Functions.ILike(x.Salesmanname, pattern, "\\") ||
                (x.Mobileno != null &&
                 EF.Functions.ILike(x.Mobileno, pattern, "\\"))       
            );
        }

        // Filter by status
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        // Count filtered records
        int total = await query.CountAsync();

        // Apply sorting and pagination
        var items = await query
            .OrderBy(x => x.Salesmancode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SalesmanResponseDto
            {
                Id = x.Id,
                Salesmancode = x.Salesmancode,
                Salesmanname = x.Salesmanname,
                Mobileno = x.Mobileno,
                Email = x.Email,
                Joiningdate = x.Joiningdate,
                Status = x.Status,
                Createddate = x.Createddate,
                Modifieddate = x.Modifieddate
            })
            .ToListAsync();

        return new PagedResponseDto<SalesmanResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }












    // Convert database entity into response DTO
    private static SalesmanResponseDto ToDto(Salesman salesman)
    {
        return new SalesmanResponseDto
        {
            Id = salesman.Id,
            Salesmancode = salesman.Salesmancode,
            Salesmanname = salesman.Salesmanname,
            Mobileno = salesman.Mobileno,
            Email = salesman.Email,
            Joiningdate = salesman.Joiningdate,
            Status = salesman.Status,
            Createddate = salesman.Createddate,
            Modifieddate = salesman.Modifieddate
        };
    }

    // 1. GET ALL

    public async Task<List<SalesmanResponseDto>> GetAllAsync()
    {
        var salesmen = await _context.Salesmen
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();

        return salesmen.Select(ToDto).ToList();
    }


    // 2. GET BY ID
    public async Task<SalesmanResponseDto?> GetByIdAsync(long id)
    {
        var salesman = await _context.Salesmen
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return salesman == null ? null : ToDto(salesman);
    }

    // 3. CREATE
    public async Task<SalesmanResponseDto> CreateAsync(
        SalesmanDto dto)
    {
        string code = dto.Salesmancode.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(dto.Salesmanname))
        {
            throw new ArgumentException(
                "Salesman code and name are required.");
        }

        ValidateRequest(dto);

        bool exists = await _context.Salesmen
            .AnyAsync(x => x.Salesmancode == code);

        if (exists)
            throw new InvalidOperationException(
                "Salesman code already exists.");

        var salesman = new Salesman
        {
            Salesmancode = code,
            Salesmanname = dto.Salesmanname.Trim(),
            Mobileno = dto.Mobileno,
            Email = dto.Email,
            Joiningdate = dto.Joiningdate,
            Status = dto.Status,
            Createddate = DateTime.UtcNow
        };

        _context.Salesmen.Add(salesman);

        await _context.SaveChangesAsync();

        return ToDto(salesman);
    }

    // 4. UPDATE
    public async Task<bool> UpdateAsync(
        long id,
        SalesmanDto dto)
    {
        var salesman = await _context.Salesmen.FindAsync(id);

        if (salesman == null)
            return false;

        string code = dto.Salesmancode.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(dto.Salesmanname))
        {
            throw new ArgumentException(
                "Salesman code and name are required.");
        }

        ValidateRequest(dto);

        bool exists = await _context.Salesmen.AnyAsync(x =>
            x.Salesmancode == code && x.Id != id);

        if (exists)
            throw new InvalidOperationException(
                "Salesman code already exists.");


        if (dto.Status != salesman.Status)
        {
            throw new InvalidOperationException(
                "Salesman status cannot be changed through Edit. " +
                "Use the Activate/Deactivate action instead.");
        }

        salesman.Salesmancode = code;
        salesman.Salesmanname = dto.Salesmanname.Trim();
        salesman.Mobileno = dto.Mobileno;
        salesman.Email = dto.Email;
        salesman.Joiningdate = dto.Joiningdate;
        //salesman.Status = dto.Status;
        salesman.Modifieddate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateRequest(SalesmanDto dto)
    {
        if (dto.Status is not 1 and not 2)
            throw new ArgumentException("Status must be 1 (Active) or 2 (Inactive).");

        if (dto.Joiningdate.HasValue && dto.Joiningdate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Joining date cannot be in the future.");
    }

    // 5. CHANGE STATUS

    public async Task<bool> ChangeStatusAsync(long id, short status)
    {
        if (status != 1 && status != 2)
        {
            throw new ArgumentException(
                "Status must be 1 (Active) or 2 (Inactive).");
        }

        var salesman = await _context.Salesmen
            .FirstOrDefaultAsync(x => x.Id == id);

        if (salesman == null)
        {
            return false;
        }

        if (status == 2)
        {
            bool isActiveTeamLeader = await _context.Marketingteams
                .AnyAsync(x =>
                    x.Teamleaderid == id &&
                    x.Status == 1);

            if (isActiveTeamLeader)
            {
                throw new InvalidOperationException(
                    "Cannot deactivate this salesman because they are the leader of an active marketing team.");
            }

            bool hasActiveAssignment = await _context.Marketingteamdetails
                .AnyAsync(x =>
                    x.Salesmanid == id &&
                    x.Status == 1);

            if (hasActiveAssignment)
            {
                throw new InvalidOperationException(
                    "Cannot deactivate this salesman because they are assigned to an active marketing team. Remove them from the team first.");
            }
        }

        salesman.Status = status;
        salesman.Modifieddate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }


    // 6. DELETE
    public async Task<bool> DeleteAsync(long id)
    {
        var salesman = await _context.Salesmen
            .FirstOrDefaultAsync(x => x.Id == id);

        if (salesman == null)
        {
            return false;
        }

        // Check whether salesman is used as a team leader
        bool isTeamLeader = await _context.Marketingteams
            .AnyAsync(x => x.Teamleaderid == id);

        if (isTeamLeader)
        {
            throw new InvalidOperationException(
                "Cannot delete this salesman because they are a marketing team leader.");
        }

        // Check whether salesman has any team assignment history
        bool hasTeamHistory = await _context.Marketingteamdetails
            .AnyAsync(x => x.Salesmanid == id);

        if (hasTeamHistory)
        {
            throw new InvalidOperationException(
                "Cannot delete this salesman because they have been assigned to a marketing team. Their assignment history must be preserved.");
        }

        _context.Salesmen.Remove(salesman);

        await _context.SaveChangesAsync();

        return true;
    }


    //7. GET ALL ACTIVE or INACTIVE SALESPEOPLE


    public async Task<List<SalesmanResponseDto>> GetByStatusAsync(short status)
    {
        var salesmen = await _context.Salesmen
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return salesmen.Select(ToDto).ToList();
    }



    //8. This returns salesmen who are active but do not have any active team assignment.
    public async Task<List<SalesmanResponseDto>> GetAvailableSalesmenAsync()
    {
        var salesmen = await _context.Salesmen
            .AsNoTracking()
            .Where(s =>
                s.Status == 1 &&
                !_context.Marketingteamdetails.Any(d =>
                    d.Salesmanid == s.Id &&
                    d.Status == 1))
            .OrderBy(s => s.Salesmanname)
            .ToListAsync();

        return salesmen.Select(ToDto).ToList();
    }


}
