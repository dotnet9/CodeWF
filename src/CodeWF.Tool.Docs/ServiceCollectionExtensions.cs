using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWF.Tool.Docs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeWFToolDocs(this IServiceCollection services)
    {
        services.AddScoped<BlazorDocService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services;
    }
}
