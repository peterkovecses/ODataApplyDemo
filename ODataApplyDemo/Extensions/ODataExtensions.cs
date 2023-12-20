using ODataApplyDemo.Models;

namespace ODataApplyDemo.Extensions;

public static class ODataExtensions
{
    public static OdataResponseWrapper ToODataResponse(this IQueryable source, string url)
        => new()
        {
            Context = url,
            Value = source
        };
}