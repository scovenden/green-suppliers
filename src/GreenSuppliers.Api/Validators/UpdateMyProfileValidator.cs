using FluentValidation;
using GreenSuppliers.Api.Models.DTOs;

namespace GreenSuppliers.Api.Validators;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.TradingName)
            .MaximumLength(200).WithMessage("Trading name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.YearFounded)
            .InclusiveBetween(1800, DateTime.UtcNow.Year)
            .WithMessage($"Year founded must be between 1800 and {DateTime.UtcNow.Year}.")
            .When(x => x.YearFounded.HasValue);

        RuleFor(x => x.EmployeeCount)
            .MaximumLength(30).WithMessage("Employee count must not exceed 30 characters.");

        RuleFor(x => x.BbbeeLevel)
            .MaximumLength(20).WithMessage("B-BBEE level must not exceed 20 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Province)
            .MaximumLength(100).WithMessage("Province must not exceed 100 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website must not exceed 500 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.");

        RuleFor(x => x.RenewableEnergyPercent)
            .InclusiveBetween(0, 100).WithMessage("Renewable energy percent must be between 0 and 100.")
            .When(x => x.RenewableEnergyPercent.HasValue);

        RuleFor(x => x.WasteRecyclingPercent)
            .InclusiveBetween(0, 100).WithMessage("Waste recycling percent must be between 0 and 100.")
            .When(x => x.WasteRecyclingPercent.HasValue);

        // Prevent excessively large lists — guard against payload bombs
        RuleFor(x => x.IndustryIds)
            .Must(ids => ids.Count <= 20).WithMessage("A supplier may be linked to at most 20 industries.");

        RuleFor(x => x.ServiceTagIds)
            .Must(ids => ids.Count <= 50).WithMessage("A supplier may have at most 50 service tags.");

        // Prevent duplicate IDs in lists
        RuleFor(x => x.IndustryIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate industry IDs are not allowed.")
            .When(x => x.IndustryIds.Count > 0);

        RuleFor(x => x.ServiceTagIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate service tag IDs are not allowed.")
            .When(x => x.ServiceTagIds.Count > 0);
    }
}
