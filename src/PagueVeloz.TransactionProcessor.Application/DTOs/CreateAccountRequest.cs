namespace PagueVeloz.TransactionProcessor.Application.DTOs;

public class CreateAccountRequest
{
    public Guid ClientId { get; set; }
    public decimal InitialBalance { get; set; } = 0;
    public decimal CreditLimit { get; set; } = 0;
}

