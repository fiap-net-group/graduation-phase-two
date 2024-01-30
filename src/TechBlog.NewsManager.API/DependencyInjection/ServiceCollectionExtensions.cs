using TechBlog.NewsManager.API.DependencyInjection.Configurations;

namespace TechBlog.NewsManager.API.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiConfiguration();

            services.AddApplicationConfiguration();

            services.AddInfrastructureConfiguration(configuration);

            services.AddSwaggerConfiguration();

            return services;
        }
    }
}
