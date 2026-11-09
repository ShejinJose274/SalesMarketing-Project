
using System.ComponentModel.DataAnnotations;

namespace SalesMarketing.API.DTOs.MarketingTeam;

public class MarketingTeamMemberDto
{
    [Required]
    public long Salesmanid { get; set; }

    [Range(1, 4)]
    public short Role { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Targetamount { get; set; }

    public short Status { get; set; } = 1;

    //public short Status { get; set; } = 1;
    // We removed the Status property because
    // Angular does not need to decide the assignment status.
}
