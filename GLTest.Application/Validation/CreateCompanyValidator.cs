using FluentValidation;
using GLTest.Application.Dtos;
using GLTest.Application.Common;


namespace GLTest.Application.Validation
{
    public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
    {
        public CreateCompanyValidator()
        {
            RuleFor(c => c.Name)
                .MaximumLength(255).WithMessage("Company name cannot exceed 255 characters.");

            RuleFor(c => c.Exchange)
                .MaximumLength(255).WithMessage("Exchange name cannot exceed 255 characters.");

            RuleFor(c => c.Ticker)
                .MaximumLength(10).WithMessage("Ticker cannot exceed 100 characters.");

            RuleFor(c => c.Isin)
                .NotEmpty().WithMessage("ISIN is required.")               
                .Matches(@"^[^0-9]{2}.*$").WithMessage("ISIN must start with two non-numeric characters.");

            RuleFor(c => c.Website)
                .Must(URLHelper.BeAValidUrl).WithMessage("Website URL is not valid.")
                .When(c => !string.IsNullOrEmpty(c.Website));
        }

    }
}
