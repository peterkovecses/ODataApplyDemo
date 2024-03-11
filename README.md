I've experienced a bug when using OData $apply. The point is that the response will be in the wrong format, not compliant with the OData protocol.

Some sample requests:

https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC))

https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC),aggregate($count%20as%20Count))

The responses:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/0794c46c-c762-4a77-9df2-82518a27273a)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/f179357e-a2c3-4b9a-a7fb-52299c112839)

As you can see, the @odata.context and value properties are missing, instead the value of the value property is returned directly.

I saw that the issue had already been reported, but I needed a quick solution.
I found that if the return value of the controller method is IEnumerable<T> or IQueriable<T> instead of IActionResult or ActionResult<T>, the response is in the correct format, but that was not an option for me, I had to find another solution. As a first step, I derived it from the EnableQueryAttribute class, and in the case of $apply, I added a marker key-value pair to the response header:

'''

    public class CustomEnableQuery : EnableQueryAttribute
    {
        private HttpContext? _httpContext;
    
        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            if (queryOptions.Apply is not null)
            {
                _httpContext!.Response.Headers.TryAdd(HeaderKeys.ODataApplyPatch, "1");
            }
            
            return queryOptions.ApplyTo(queryable);
        }
    
        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            _httpContext = request.HttpContext;
            base.ValidateQuery(request, queryOptions);
        }
    }
'''

Next, I created a wrapper class:

'''

    public class OdataResponseWrapper
    {
        [JsonPropertyName("@odata.context")]
        public required string Context { get; set; }
    
        [JsonPropertyName("value")]
        public required object[] Value { get; set; }
    }
'''

I monitor the marker header in a middleware, and if it appears, I wrap the value in the response body so that it complies with the OData protocol.

'''

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
'''

In addition, I only had to register the middleware in the Program.cs:
'''

    app.UseMiddleware<ODataApplyPatchMiddleware>();
'''

And the results:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/2146f464-a630-4a5f-9839-8cc91bed4c8a)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/a9d78e6e-ed97-4fd6-9f67-7b8d4296ba30)



