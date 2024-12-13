using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using System.Reflection;
using DotnetBeBase.Configurations;
using DotnetBeBase.SecurityManagers;

namespace Tư.Configurations
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection _service, string _title, string _version)
        {
            _service.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Nhập token vào đây:",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                opt.OperationFilter<SecurityRequirementsOperationFilter>();
            });
            _service.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader()
                                                                //new HeaderApiVersionReader("x-api-version")
                                                                //new MediaTypeApiVersionReader("x-api-version")
                                                                );
            });
            // Add ApiExplorer to discover versions
            _service.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            _service.ConfigureOptions<ConfigureSwaggerOptions>();


            return _service;
        }

        public static void ConfigurationSwaggerUI(this WebApplication _app)
        {
            var apiVersionDescriptionProvider = _app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            _app.UseSwagger();
            _app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Client");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "Admin");
            });
        }
        public static IApplicationBuilder UseCustomPrefix(this IApplicationBuilder _app, string prefix)
        {
            _app.UsePathBase($"/{prefix}");
            _app.Use((context, next) =>
            {
                context.Request.PathBase = $"/{prefix}";
                return next();
            });
            return _app;
        }
    }
}
