using FluentValidation;
using GreenSuppliers.Api.Models.DTOs;

namespace GreenSuppliers.Api.Validators;

public class AddCertificationValidator : AbstractValidator<AddCertificationRequest>
{
    public AddCertificationValidator()
    {
        RuleFor(x => x.CertificationTypeId)
            .NotEmpty().WithMessage("Certification type is required.");

        RuleFor(x => x.CertificateNumber)
            .MaximumLength(100).WithMessage("Certificate number must not exceed 100 characters.");

        RuleFor(x => x.IssuedAt)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Issue date cannot be in the future.")
            .When(x => x.IssuedAt.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.IssuedAt)
            .WithMessage("Expiry date must be after the issue date.")
            .When(x => x.IssuedAt.HasValue && x.ExpiresAt.HasValue);

        RuleFor(x => x.DocumentId)
            .NotEqual(Guid.Empty).WithMessage("Document ID must not be an empty GUID.")
            .When(x => x.DocumentId.HasValue);
    }
}
