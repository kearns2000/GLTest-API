
namespace GLTest.Application.Dtos
{
    public class CreateCompanyDto
    {     
        public string Name { get; set; } = string.Empty;     
        public string Exchange { get; set; } = string.Empty;       
        public string Ticker { get; set; } = string.Empty;   
        public string Isin { get; set; } = string.Empty;      
        public string? Website { get; set; }
    }
}
