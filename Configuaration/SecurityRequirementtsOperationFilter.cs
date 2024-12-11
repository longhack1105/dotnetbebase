using System;
using System.Web.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TWChatAppApiMaster.Configuaration
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext _context)
        {
            var _HasAuthorize = _context.MethodInfo.DeclaringType != null
                && (_context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || _context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());
            if (_HasAuthorize)
            {
                //- 400 if the request not found.
                //- 401 if the authentication middleware runs and the user is not authenticated.
                //- 403 if the authorization middleware runs and the user is unauthenticated,
                //      claim value not authroised, scope not authorized,
                //      user doesnt have required claim or cannot find claim.
                //- 404 if unable to find a route.
                //- 499 if the request is cancelled by the client.
                //- 500 if unable to complete the HTTP request and the exception is not OperationCanceledException or HttpRequestException.
                //- 502 if unable to connect to service.
                //- 503 if the request times out.
                //operation.Responses.Add("400", new OpenApiResponse { Description = "Bad Request" });                
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                //operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
                //operation.Responses.Add("404", new OpenApiResponse { Description = "Unable find of route" });
                //operation.Responses.Add("499", new OpenApiResponse { Description = "Reuqest is cancelled" });
                //operation.Responses.Add("500", new OpenApiResponse { Description = "Internal Server Error" });
                //operation.Responses.Add("502", new OpenApiResponse { Description = "Bad Gateway" });
                //operation.Responses.Add("503", new OpenApiResponse { Description = "Service Unavailable" });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Product",
                    In = ParameterLocation.Header,
                    Description = "Tên sản phẩm",
                    Required = true,
                    AllowEmptyValue = false
                });
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }
                        ] = new string[]{}
                    }
                };
            }

            //throw new NotImplementedException();
        }
    }
}
