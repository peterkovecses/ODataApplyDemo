using Microsoft.AspNetCore.OData.Query;
using ODataApplyDemo.Constants;

namespace ODataApplyDemo.Attributes;

public class CustomEnableQuery : EnableQueryAttribute
{
    private HttpContext? _httpContext;

    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        if (queryOptions.Apply is not null)
        {
            _httpContext!.Response.Headers.TryAdd(HeaderKeys.ODataApplyPatch, "$1");
        }
        
        return queryOptions.ApplyTo(queryable);
    }

    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        _httpContext = request.HttpContext;
        base.ValidateQuery(request, queryOptions);
    }
}