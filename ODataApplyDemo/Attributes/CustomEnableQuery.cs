using Microsoft.AspNetCore.OData.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace ODataApplyDemo.Attributes;

public class CustomEnableQuery : EnableQueryAttribute
{
    private string _fullUrl = string.Empty;

    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        var query = queryOptions.ApplyTo(queryable);
        if (queryOptions.Apply is null) return query;
        var result = query.ToODataResponse(_fullUrl);

        return result;
    }

    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        _fullUrl = request.FullUrl();
        base.ValidateQuery(request, queryOptions);
    }
}

public static class HttpRequestExtensions
{
    public static string FullUrl(this HttpRequest request)
        => $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
}

public static class ODataExtensions
{
    public static ODataResponseHelper ToODataResponse(this IQueryable source, string url)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        return new()
        {
            Context = url,
            Value = source
        };
    }
}

public class ODataResponseHelper : IQueryable
{
    public required string Context { get; set; }

    public required IQueryable Value { get; set; }

    public IEnumerator GetEnumerator()
    {
        yield return new OdataResponseWrapper
        {
            Context = this.Context,
            Value = this.Value
        };
    }

    public Type ElementType => typeof(ODataResponseHelper);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => Value.Provider;
}

public class OdataResponseWrapper
{
    [JsonPropertyName("@odata.context")]
    public required string Context { get; set; }

    [JsonPropertyName("value")]
    public required IQueryable Value { get; set; }
}
