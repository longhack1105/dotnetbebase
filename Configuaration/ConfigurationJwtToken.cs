using ChatApp.Configuaration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TWChatAppApiMaster.SecurityManagers;

namespace TWChatAppApiMaster.Configurations
{
    public static class ConfigurationJwtToken
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection _service)
        {
            _service.AddSingleton<IJwtAuthenticationManager>(new JwtAuthenticationManager());
             _service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidAudience = GlobalSettings.AppSettings.TokenSettings.ValidAudience,
                    ValidIssuer = GlobalSettings.AppSettings.TokenSettings.ValidIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalSettings.AppSettings.TokenSettings.Secret))
                };
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            });
            return _service;
        }
    }
}
