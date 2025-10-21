namespace PagueVeloz.TransactionProcessor.Application.DTOs;

public class CreateAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Balance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

