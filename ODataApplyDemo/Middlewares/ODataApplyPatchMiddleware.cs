﻿using ODataApplyDemo.Extensions;
using ODataApplyDemo.Models;
using System.Text.Json;
using ODataApplyDemo.Constants;

namespace ODataApplyDemo.Middlewares;

public class ODataApplyPatchMiddleware(RequestDelegate next)
{
    public const string QueryDelimiter = "?";
    public const string ForwardSlash = "/";

    public async Task InvokeAsync(HttpContext context)
    {
        var originalResponseBodyStream = context.Response.Body;
        using var updatedBodyStream = new MemoryStream();
        context.Response.Body = updatedBodyStream;

        await next(context);

        if (context.Response.Headers.TryGetValue(HeaderKeys.ODataApplyPatch, out _))
        {
            context.Response.Headers.Remove(HeaderKeys.ODataApplyPatch);
            await UpdateResponseBodyAsync(context.Response, updatedBodyStream , context.Request.FullUrl());
        }

        await FinalizeResponseBody(updatedBodyStream, originalResponseBodyStream, context.Response);
    }

    private static async Task UpdateResponseBodyAsync(HttpResponse response, Stream updatedBodyStream, string requestUrl)
    {
        var stream = response.Body;
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(updatedBodyStream).ReadToEndAsync();
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        var jsonContent = GenerateODataResponseContent(requestUrl, responseBody);
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        await writer.WriteAsync(jsonContent);
        await writer.FlushAsync();
        response.ContentLength = stream.Length;
    }

    private static string GenerateODataResponseContent(string requestUrl, string responseBody)
    {
        var value = JsonSerializer.Deserialize<object[]>(responseBody);
        var uriWithoutQuery = requestUrl.Contains(QueryDelimiter) ? requestUrl[..requestUrl.IndexOf(QueryDelimiter, StringComparison.Ordinal)] : requestUrl;
        var urlParts = uriWithoutQuery.Split(ForwardSlash);
        var endpoint = urlParts.Last();
        var url = string.Join(ForwardSlash, urlParts.Take(urlParts.Length - 1));

        var odataResponse = new OdataResponseWrapper
        {
            Context = $"{url}/$metadata#{endpoint}",
            Value = value!
        };

        return JsonSerializer.Serialize(odataResponse);
    }

    private static async Task FinalizeResponseBody(Stream updatedBodyStream, Stream originalResponseBodyStream,
        HttpResponse response)
    {
        updatedBodyStream.Seek(0, SeekOrigin.Begin);
        await updatedBodyStream.CopyToAsync(originalResponseBodyStream);
        response.Body = originalResponseBodyStream;
    }
}


