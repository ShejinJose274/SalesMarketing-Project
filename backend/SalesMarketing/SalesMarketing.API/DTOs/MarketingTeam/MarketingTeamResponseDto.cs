
namespace SalesMarketing.API.DTOs.MarketingTeam;

public class MarketingTeamResponseDto
{
    public long Id { get; set; }

    public string Teamcode { get; set; } = string.Empty;

    public string Teamname { get; set; } = string.Empty;

    public long Teamleaderid { get; set; }

    public string Teamleadername { get; set; } = string.Empty;

    public string? Description { get; set; }

    public short Status { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Modifieddate { get; set; }

    public List<MarketingTeamMemberResponseDto> Members { get; set; } = new();
}

public class MarketingTeamMemberResponseDto
{
    public long Salesmanid { get; set; }

    public string Salesmanname { get; set; } = string.Empty;

    public short Role { get; set; }

    public decimal Targetamount { get; set; }

    public short Status { get; set; }
}
