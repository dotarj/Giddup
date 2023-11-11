// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Giddup.Presentation.Api.AppStartup;

public static class Authentication
{
    public static IServiceCollection AddAppStartupAuthentication(this IServiceCollection services)
    {
        _ = services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Audience = "giddup.api";

                // NOTE: Disabling validation of issuer and issuer signing key is only for demo purposes and should NEVER end
                // up in a production environment!
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = false,
                    SignatureValidator = (token, _) => new JwtSecurityToken(token)
                };
            });

        return services;
    }

    public static WebApplication UseAppStartupAuthentication(this WebApplication app)
    {
        _ = app.UseAuthentication();

        return app;
    }
}
