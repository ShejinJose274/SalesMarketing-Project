
namespace SalesMarketing.API.DTOs.Salesman;

public class SalesmanResponseDto
{
    public long Id { get; set; }

    public string Salesmancode { get; set; } = string.Empty;

    public string Salesmanname { get; set; } = string.Empty;

    public string? Mobileno { get; set; }

    public string? Email { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public short Status { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Modifieddate { get; set; }
}
