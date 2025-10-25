using FluentValidation;
using PagueVeloz.TransactionProcessor.Application.DTOs;

namespace PagueVeloz.TransactionProcessor.Application.Validators;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("ClientId é obrigatório")
            .NotEqual(Guid.Empty)
            .WithMessage("ClientId deve ser um GUID válido");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Saldo inicial deve ser maior ou igual a zero")
            .LessThan(1000000)
            .WithMessage("Saldo inicial não pode exceder R$ 1.000.000,00");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Limite de crédito deve ser maior ou igual a zero")
            .LessThan(10000000)
            .WithMessage("Limite de crédito não pode exceder R$ 10.000.000,00");

        // Validação adicional: se o saldo inicial for maior que o limite de crédito
        RuleFor(x => x)
            .Must(x => x.InitialBalance <= x.CreditLimit)
            .WithMessage("Saldo inicial não pode ser maior que o limite de crédito");
    }
}
