

using FluentValidation;
using GLTest.Application.Common;
using GLTest.Application.Dtos;
using GLTest.Application.ExtensionMethods;
using GLTest.Infrastructure.Repositories;


namespace GLTest.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repository;
        private readonly IValidator<UpdateCompanyDto> _updateBusinessValidator;
        private readonly IValidator<CreateCompanyDto> _createBusinessValidator;

        public CompanyService(ICompanyRepository repository,
            IValidator<UpdateCompanyDto> updateBusinessValidator,
            IValidator<CreateCompanyDto> createBusinessValidator)
        {
            _repository = repository;
            _updateBusinessValidator = updateBusinessValidator;
            _createBusinessValidator = createBusinessValidator;
        }

        public async Task<List<CompanyDto>> GetAllCompanies(CancellationToken cancellationToken)
        {
            return (await _repository.GetAllAsync(cancellationToken)).ToDtoList();
        }

        public async Task<CompanyDto?> GetCompanyById(Guid id, CancellationToken cancellationToken)
        {
            return (await _repository.GetByIdAsync(id, cancellationToken))?.ToDto();
        }

        public async Task<CompanyDto?> GetCompanyByIsin(string isin, CancellationToken cancellationToken)
        {
            return (await _repository.GetByIsinAsync(isin, cancellationToken))?.ToDto();
        }

        public async Task<Result<CompanyDto>> CreateCompany(CreateCompanyDto createCompanyDto, CancellationToken cancellationToken)
        {
            var validationResult = await _createBusinessValidator.ValidateAsync(createCompanyDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<CompanyDto>.FailureResult("Validation failed", MapErrors(validationResult));
            }

            if (await _repository.GetByIsinAsync(createCompanyDto.Isin, cancellationToken) is not null)
            {
                return Result<CompanyDto>.FailureResult("Validation failed", new Dictionary<string, string[]> { { "Isin", new[] { "Company with the same ISIN already exists" } } });
            }

            var entity = createCompanyDto.ToEntity();
            await _repository.AddAsync(entity, cancellationToken);
            return Result<CompanyDto>.SuccessResult(entity.ToDto(), "Company created successfully.");
        }

        public async Task<Result<CompanyDto>> UpdateCompany(UpdateCompanyDto updateCompanyDto, CancellationToken cancellationToken)
        {
            var validationResult = await _updateBusinessValidator.ValidateAsync(updateCompanyDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<CompanyDto>.FailureResult("Validation failed", MapErrors(validationResult));
            }

            var entity = updateCompanyDto.ToEntity();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<CompanyDto>.SuccessResult(entity.ToDto(), "Company updated successfully.");
        }

        private static Dictionary<string, string[]> MapErrors(FluentValidation.Results.ValidationResult validationResult)
        {
            return validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
    }
}
