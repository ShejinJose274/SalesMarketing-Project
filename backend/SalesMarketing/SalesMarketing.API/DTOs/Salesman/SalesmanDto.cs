
using System.ComponentModel.DataAnnotations;

namespace SalesMarketing.API.DTOs.Salesman;

public class SalesmanDto
{
    [Required, StringLength(20), RegularExpression("^[A-Za-z0-9]+$")]
    public string Salesmancode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Salesmanname { get; set; } = string.Empty;

    [StringLength(20), RegularExpression("^[0-9]{7,15}$")]
    public string? Mobileno { get; set; }

    [StringLength(100), EmailAddress]
    public string? Email { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public short Status { get; set; } = 1;
}
