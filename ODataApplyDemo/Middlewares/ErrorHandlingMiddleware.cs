using ODataApplyDemo.Exceptions;
using ODataApplyDemo.JsonConverters;
using System.Net;
using System.Text.Json;

namespace ODataApplyDemo.Middlewares;

public class ErrorHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ODataApplyException ex)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new AggregationWrapperConverter() }
            };
            var jsonContent = JsonSerializer.Serialize(ex.Response, jsonOptions);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteAsync(jsonContent);
        }
    }
}
