using ODataApplyDemo.Extensions;
using ODataApplyDemo.Models;
using System.Text.Json;
using ODataApplyDemo.Constants;

namespace ODataApplyDemo.Middlewares;

public class ODataApplyPatchMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;
        var originalResponseBodyStream = response.Body;
        using var newResponseBody = new MemoryStream();
        response.Body = newResponseBody;

        await next(context);

        if (context.Response.Headers.TryGetValue(HeaderKeys.ODataApplyPatch, out _))
        {
            context.Response.Headers.Remove(HeaderKeys.ODataApplyPatch);
            await ModifyResponseAsync(response, newResponseBody, context.Request.FullUrl());
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBodyStream);
        response.Body = originalResponseBodyStream;
    }

    private async Task ModifyResponseAsync(HttpResponse response, MemoryStream newBody, string fullUrl)
    {
        var stream = response.Body;
        using var reader = new StreamReader(stream, leaveOpen: true);
        var originalResponse = await reader.ReadToEndAsync();
        newBody.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(newBody).ReadToEnd();
        newBody.Seek(0, SeekOrigin.Begin);
        var value = JsonSerializer.Deserialize<object[]>(responseBody);
        var odataResponse = new OdataResponseWrapper
        {
            Context = fullUrl,
            Value = value!
        };
        var jsonContent = JsonSerializer.Serialize(odataResponse);
        stream.SetLength(0);
        using var writer = new StreamWriter(stream, leaveOpen: true);
        await writer.WriteAsync(jsonContent);
        await writer.FlushAsync();
        response.ContentLength = stream.Length;
    }
}