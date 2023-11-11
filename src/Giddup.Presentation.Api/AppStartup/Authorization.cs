// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Presentation.Api.AppStartup;

public static class Authorization
{
    public static WebApplication UseAppStartupAuthorization(this WebApplication app)
    {
        _ = app.UseAuthorization();

        return app;
    }
}
