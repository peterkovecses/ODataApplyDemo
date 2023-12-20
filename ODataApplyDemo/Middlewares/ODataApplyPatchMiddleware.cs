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
        using var updatedBodyStream = new MemoryStream();
        response.Body = updatedBodyStream;

        await next(context);

        if (context.Response.Headers.TryGetValue(HeaderKeys.ODataApplyPatch, out _))
        {
            context.Response.Headers.Remove(HeaderKeys.ODataApplyPatch);
            await UpdateResponseBodyAsync(response, updatedBodyStream , context.Request.FullUrl());
        }

        await FinalizeResponseBody(updatedBodyStream, originalResponseBodyStream, response);
    }

    private static async Task UpdateResponseBodyAsync(HttpResponse response, Stream updatedBodyStream, string requestUrl)
    {
        var stream = response.Body;
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(updatedBodyStream).ReadToEndAsync();
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        var jsonContent = GenerateODataResponseContent(requestUrl, responseBody);
        stream.SetLength(0);
        using var writer = new StreamWriter(stream, leaveOpen: true);
        await writer.WriteAsync(jsonContent);
        await writer.FlushAsync();
        response.ContentLength = stream.Length;
    }

    private static string GenerateODataResponseContent(string requestUrl, string responseBody)
    {
        var value = JsonSerializer.Deserialize<object[]>(responseBody);
        var odataResponse = new OdataResponseWrapper
        {
            Context = requestUrl,
            Value = value!
        };
        var jsonContent = JsonSerializer.Serialize(odataResponse);
        return jsonContent;
    }

    private static async Task FinalizeResponseBody(Stream updatedBodyStream, Stream originalResponseBodyStream,
        HttpResponse response)
    {
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        await updatedBodyStream.CopyToAsync(originalResponseBodyStream);
        response.Body = originalResponseBodyStream;
    }
}


