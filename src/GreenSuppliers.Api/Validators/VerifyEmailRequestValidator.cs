using FluentValidation;
using GreenSuppliers.Api.Models.DTOs;

namespace GreenSuppliers.Api.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(128).WithMessage("Token must not exceed 128 characters.");
    }
}
