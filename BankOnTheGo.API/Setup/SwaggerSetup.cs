using Microsoft.OpenApi.Models;

namespace BankOnTheGo.API.Setup;

public static class SwaggerSetup
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "BankOnTheGo", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter a valid JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
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
        });
        return services;
    }

    public static void UseSwaggerDocs(this WebApplication app)
    {
        app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = string.Empty;
            c.SwaggerEndpoint("./swagger/v1/swagger.json", "BankOnTheGo API v1");
        });

        app.MapGet("/v1/swagger.json", ctx =>
        {
            ctx.Response.Redirect("/swagger/v1/swagger.json", permanent: false);
            return Task.CompletedTask;
        });
    }
}