
using System.ComponentModel.DataAnnotations;

namespace SalesMarketing.API.DTOs.MarketingTeam;

public class MarketingTeamDto
{
    [Required, StringLength(20), RegularExpression("^[A-Za-z0-9]+$")]
    public string Teamcode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Teamname { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long Teamleaderid { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public List<MarketingTeamMemberDto> Members { get; set; } = new();

    public short Status { get; set; } = 1;
}
