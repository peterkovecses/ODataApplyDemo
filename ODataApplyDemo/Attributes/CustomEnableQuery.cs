using Microsoft.AspNetCore.OData.Query;
using ODataApplyDemo.Exceptions;
using ODataApplyDemo.Extensions;

namespace ODataApplyDemo.Attributes;

public class CustomEnableQuery : EnableQueryAttribute
{
    private string _fullUrl = string.Empty;

    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        var query = queryOptions.ApplyTo(queryable);
        if (queryOptions.Apply is null) return query;
        throw new ODataApplyException(query.ToODataResponse(_fullUrl));
    }

    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        _fullUrl = request.FullUrl();
        base.ValidateQuery(request, queryOptions);
    }
}