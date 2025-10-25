using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.TransactionProcessor.Domain.Entities;

public class Client
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}

public enum ClientStatus
{
    Active,
    Inactive,
    Blocked
}

