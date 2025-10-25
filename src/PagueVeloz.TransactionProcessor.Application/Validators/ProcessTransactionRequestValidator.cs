using FluentValidation;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Application.Validators;

public class ProcessTransactionRequestValidator : AbstractValidator<ProcessTransactionRequest>
{
    private static readonly string[] AllowedCurrencies = { "BRL", "USD", "EUR" };
    private static readonly OperationType[] ValidOperations = 
    {
        OperationType.Credit,
        OperationType.Debit,
        OperationType.Reserve,
        OperationType.Capture,
        OperationType.Reversal,
        OperationType.Transfer
    };

    public ProcessTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("AccountId é obrigatório")
            .NotEqual(Guid.Empty)
            .WithMessage("AccountId deve ser um GUID válido");

        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("ReferenceId é obrigatório")
            .Length(1, 100)
            .WithMessage("ReferenceId deve ter entre 1 e 100 caracteres")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("ReferenceId deve conter apenas letras, números, hífens e underscores");

        RuleFor(x => x.Operation)
            .IsInEnum()
            .WithMessage("Operação deve ser um tipo válido")
            .Must(operation => ValidOperations.Contains(operation))
            .WithMessage("Operação deve ser Credit, Debit, Reserve, Capture, Reversal ou Transfer");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero")
            .LessThan(10000000)
            .WithMessage("Valor não pode exceder R$ 10.000.000,00")
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Valor deve ter no máximo 2 casas decimais");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Moeda é obrigatória")
            .Length(3)
            .WithMessage("Moeda deve ter exatamente 3 caracteres")
            .Must(currency => AllowedCurrencies.Contains(currency.ToUpper()))
            .WithMessage("Moeda deve ser BRL, USD ou EUR");

        RuleFor(x => x.Metadata)
            .Must(metadata => metadata == null || IsValidMetadata(metadata))
            .WithMessage("Metadata contém dados inválidos ou maliciosos");

        // Validações específicas para transferência
        RuleFor(x => x.DestinationAccountId)
            .NotEmpty()
            .WithMessage("Conta de destino é obrigatória para transferências")
            .NotEqual(Guid.Empty)
            .WithMessage("Conta de destino deve ser um GUID válido")
            .When(x => x.Operation == OperationType.Transfer);

        RuleFor(x => x.DestinationAccountId)
            .NotEqual(x => x.AccountId)
            .WithMessage("Conta de origem e destino não podem ser iguais")
            .When(x => x.Operation == OperationType.Transfer);

        // Validações específicas para reversão
        RuleFor(x => x.OriginalReferenceId)
            .NotEmpty()
            .WithMessage("ReferenceId original é obrigatório para reversões")
            .Length(1, 100)
            .WithMessage("ReferenceId original deve ter entre 1 e 100 caracteres")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("ReferenceId original deve conter apenas letras, números, hífens e underscores")
            .When(x => x.Operation == OperationType.Reversal);
    }

    private static bool IsValidMetadata(Dictionary<string, object> metadata)
    {
        if (metadata == null) return true;

        // Verificar se o JSON serializado não excede o limite
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        if (json.Length > 4000)
            return false;

        // Verificar se não contém caracteres suspeitos
        var suspiciousPatterns = new[] { "<script", "javascript:", "onload=", "onerror=", "eval(", "function(" };
        return !suspiciousPatterns.Any(pattern => 
            json.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
