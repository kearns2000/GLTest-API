using FluentValidation;
using GLTest.Application.Common;
using GLTest.Application.Dtos;

namespace GLTest.Application.Validation
{
    public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Company ID is required.");

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
