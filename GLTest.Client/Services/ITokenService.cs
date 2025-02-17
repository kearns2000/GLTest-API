namespace GLTest.Client.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
    }
}

