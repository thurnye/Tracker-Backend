using FluentValidation;
using DonationsTracker.DB;

public class DonationValidator : AbstractValidator<Donation>
{
    public DonationValidator()
    {
        // RuleFor(x => x.DonorName)
        //     .NotEmpty().WithMessage("Donor name is required.")
        //     .MaximumLength(100)
        //     .Matches("^[a-zA-Z0-9 '.,-]*$").WithMessage("Invalid characters in donor name.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .Must(desc => !desc.Contains("<script", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Script tags are not allowed.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Donation amount must be greater than zero.");
    }
}
