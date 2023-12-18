﻿using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ODataApplyDemo.Swagger;

public class IgnoreODataQueryOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        context.ApiDescription.ParameterDescriptions
            .Where(desc => desc?.ParameterDescriptor?.ParameterType?.BaseType == typeof(ODataQueryOptions)).ToList()
            .ForEach(param =>
            {
                var toRemove = operation.Parameters.SingleOrDefault(openApiParameter => openApiParameter.Name == param.Name);
                if (null != toRemove)
                {
                    _ = operation.Parameters.Remove(toRemove);
                }
            });
    }
}
