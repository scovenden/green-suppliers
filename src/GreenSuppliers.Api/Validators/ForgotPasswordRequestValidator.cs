using FluentValidation;
using GreenSuppliers.Api.Models.DTOs;

namespace GreenSuppliers.Api.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.");
    }
}
