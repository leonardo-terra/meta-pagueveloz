using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.TransactionProcessor.Domain.Entities;

public class Account
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ClientId { get; set; }
    
    public decimal Balance { get; set; } = 0;
    
    public decimal ReservedBalance { get; set; } = 0;
    
    public decimal CreditLimit { get; set; } = 0;
    
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Client Client { get; set; } = null!;
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
    // Calculated properties
    public decimal AvailableBalance => Balance - ReservedBalance;
    
    public decimal TotalAvailableBalance => AvailableBalance + CreditLimit;
    
    // Business methods
    public bool CanDebit(decimal amount)
    {
        return TotalAvailableBalance >= amount;
    }
    
    public bool CanReserve(decimal amount)
    {
        return AvailableBalance >= amount;
    }
    
    public bool CanCapture(decimal amount)
    {
        return ReservedBalance >= amount;
    }
}

public enum AccountStatus
{
    Active,
    Inactive,
    Blocked
}

