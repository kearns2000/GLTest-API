using FluentValidation;
using GLTest.Application.Common;
using GLTest.Application.Dtos;
using GLTest.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLTest.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/companies")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IValidator<CreateCompanyDto> _createValidator;
        private readonly IValidator<UpdateCompanyDto> _updateValidator;

        public CompanyController(
            ICompanyService companyService,
            IValidator<CreateCompanyDto> createValidator,
            IValidator<UpdateCompanyDto> updateValidator)
        {
            _companyService = companyService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<CompanyDto>>>> GetAll(CancellationToken cancellationToken)
        {
            var companies = await _companyService.GetAllCompanies(cancellationToken);
            return Ok(Result<List<CompanyDto>>.SuccessResult(companies ?? new List<CompanyDto>(), "Companies retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<CompanyDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var company = await _companyService.GetCompanyById(id, cancellationToken);
            return company == null
                ? NotFound(Result<CompanyDto>.FailureResult("Company not found"))
                : Ok(Result<CompanyDto>.SuccessResult(company, "Company retrieved successfully"));
        }

        [HttpGet("isin/{isin}")]
        public async Task<ActionResult<Result<CompanyDto>>> GetByIsin(string isin, CancellationToken cancellationToken)
        {
            var company = await _companyService.GetCompanyByIsin(isin, cancellationToken);
            return company == null
                ? NotFound(Result<CompanyDto>.FailureResult("Company not found"))
                : Ok(Result<CompanyDto>.SuccessResult(company, "Company retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto companyDto, CancellationToken cancellationToken)
        {
            var validationResult = await _createValidator.ValidateAsync(companyDto, cancellationToken);
            if (!validationResult.IsValid)
                return HandleValidationErrors(validationResult);

            var result = await _companyService.CreateCompany(companyDto, cancellationToken);
            if (!result.Success)
                return HandleBusinessValidationErrors(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, Result<bool>.SuccessResult(true, "Company created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto companyDto, CancellationToken cancellationToken)
        {
            if (id != companyDto.Id)
                return BadRequest(Result<object>.FailureResult("ID mismatch"));

            var validationResult = await _updateValidator.ValidateAsync(companyDto, cancellationToken);
            if (!validationResult.IsValid)
                return HandleValidationErrors(validationResult);

            var result = await _companyService.UpdateCompany(companyDto, cancellationToken);
            if (!result.Success)
                return HandleBusinessValidationErrors(result);

            return Ok(Result<bool>.SuccessResult(true, "Company updated successfully."));
        }

        private IActionResult HandleValidationErrors(FluentValidation.Results.ValidationResult validationResult)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext?.Request?.Path.ToString()
            };

            problemDetails.Extensions["errors"] = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
        }

        private IActionResult HandleBusinessValidationErrors(Result<CompanyDto> result)
        {
            var errorMessage = "Unknown validation error occurred.";

            if (result.Errors != null && result.Errors.Any())
            {
                var firstError = result.Errors.First();
                errorMessage = $"{firstError.Key}: {string.Join(", ", firstError.Value)}";
            }

            var problemDetails = new ProblemDetails
            {
                Title = "Business validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = errorMessage,
                Instance = HttpContext?.Request?.Path.ToString()
            };

            problemDetails.Extensions["errors"] = result.Errors ?? new Dictionary<string, string[]>();

            return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
                      
        }
    }
}

