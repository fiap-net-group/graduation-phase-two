using PoliceDepartment.EvidenceManager.API.Middlewares;
using System.Text.Json.Serialization;
using TechBlog.NewsManager.API.Endpoints;

namespace TechBlog.NewsManager.API.DependencyInjection.Configurations
{
    public static class ApiConfiguration
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this WebApplication app, bool isDevelopment) 
        { 
            app.UseRouting();

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.MapBlogUsersEndpoints();
            app.MapBlogNewsEndpoints();
            app.MapAuthenticationEndpoints();

            if (!isDevelopment)
            {
                app.UseMiddleware<ApiKeyMiddleware>();
            }

            return app;
        }
    }
}
