namespace ODataApplyDemo.Extensions;

public static class HttpRequestExtensions
{
    public static string FullUrl(this HttpRequest request)
        => $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
}