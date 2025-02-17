using GLTest.Domain.Entities;
using GLTest.Application.Dtos;

namespace GLTest.Application.ExtensionMethods
{
 
        public static class MappingExtensions
        {
            public static CompanyDto ToDto(this Company company)
            {
                return new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Exchange = company.Exchange,
                    Ticker = company.Ticker,
                    Isin = company.Isin,
                    Website = company.Website,
                    DateCreated = company.DateCreated,
                    DateUpdated = company.DateUpdated,
                    UpdatedBy = company.UpdatedBy
                };
            }

            public static Company ToEntity(this CreateCompanyDto Dto)
            {
                return new Company
                {
                    Id = Guid.NewGuid(), 
                    Name = Dto.Name,
                    Exchange = Dto.Exchange,
                    Ticker = Dto.Ticker,
                    Isin = Dto.Isin,
                    Website = Dto.Website,
                    DateCreated = DateTime.UtcNow
                };
            }

        public static Company ToEntity(this UpdateCompanyDto Dto)
        {
            return new Company
            {
                Id =Dto.Id, 
                Name = Dto.Name,
                Exchange = Dto.Exchange,
                Ticker = Dto.Ticker,
                Isin = Dto.Isin,
                Website = Dto.Website,
                UpdatedBy = Dto.UpdatedBy,
            };
        }


        public static List<CompanyDto> ToDtoList(this IEnumerable<Company> companies)
            {
                return companies.Select(company => company.ToDto()).ToList();
            }
        }
  
}
