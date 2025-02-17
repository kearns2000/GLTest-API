
namespace GLTest.Application.Common
{
    public class URLHelper 
    { 
        public static bool BeAValidUrl(string? url)
        {
            return !string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
