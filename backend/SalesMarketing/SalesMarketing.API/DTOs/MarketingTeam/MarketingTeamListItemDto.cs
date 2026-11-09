namespace SalesMarketing.API.DTOs.MarketingTeam;

public sealed class MarketingTeamListItemDto
{
    public long Id { get; set; }
    public string Teamcode { get; set; } = string.Empty;
    public string Teamname { get; set; } = string.Empty;
    public long Teamleaderid { get; set; }
    public string Teamleadername { get; set; } = string.Empty;
    public int Salesmancount { get; set; }
    public short Status { get; set; }
}
