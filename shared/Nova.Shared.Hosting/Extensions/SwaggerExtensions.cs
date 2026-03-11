using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nova.Shared.Hosting.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddNovaSwagger(this IServiceCollection services, string title = "Nova API")
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
            options.DocInclusionPredicate((_, _) => true);
            options.CustomSchemaIds(type => type.FullName);
            IncludeNovaXmlComments(options);
            AddBearerSecurity(options);
        });

        return services;
    }

    /// <summary>
    /// 多文档分组模式：每个 (docName, title, namespacePrefix) 注册为独立 SwaggerDoc，
    /// 通过 Controller 类型所在命名空间自动归类到对应分组。
    /// </summary>
    public static IServiceCollection AddNovaSwaggerMultiDoc(
        this IServiceCollection services,
        params (string Name, string Title, string NamespacePrefix)[] docs)
    {
        services.AddAbpSwaggerGen(options =>
        {
            foreach (var (name, title, _) in docs)
            {
                options.SwaggerDoc(name, new OpenApiInfo { Title = title, Version = "v1" });
            }

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                var ns = string.Empty;

                if (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerAction)
                    ns = controllerAction.ControllerTypeInfo.FullName ?? string.Empty;
                else
                    ns = apiDesc.ActionDescriptor.DisplayName ?? string.Empty;

                foreach (var doc in docs)
                {
                    if (ns.Contains(doc.NamespacePrefix, StringComparison.OrdinalIgnoreCase))
                        return docName == doc.Name;
                }

                return docName == docs[0].Name;
            });

            options.CustomSchemaIds(type => type.FullName);
            IncludeNovaXmlComments(options);
            AddBearerSecurity(options);
        });

        return services;
    }

    private static void IncludeNovaXmlComments(SwaggerGenOptions options)
    {
        foreach (var xmlPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "Nova*.xml", SearchOption.TopDirectoryOnly))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }

    private static void AddBearerSecurity(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header (Bearer {token})",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    }
}
