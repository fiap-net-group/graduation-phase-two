using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechBlog.NewsManager.API.Domain.Authentication;
using TechBlog.NewsManager.API.Domain.Database;
using TechBlog.NewsManager.API.Domain.Entities;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Domain.ValueObjects;
using TechBlog.NewsManager.API.Infrastructure.Authentication.Configuration.Context;
using TechBlog.NewsManager.API.Infrastructure.Database;
using TechBlog.NewsManager.API.Infrastructure.Database.Context;
using TechBlog.NewsManager.API.Infrastructure.Database.Repositories;
using TechBlog.NewsManager.API.Infrastructure.Identity;
using TechBlog.NewsManager.API.Infrastructure.Logger;

namespace TechBlog.NewsManager.API.DependencyInjection.Configurations
{
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILoggerManager, ConsoleLogger>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IBlogNewsRepository, BlogNewsRepository>();

            services.AddScoped<IDatabaseContext, SqlServerContext>();
            services.AddDbContext<IDatabaseContext, SqlServerContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            });

            services.AddScoped(o => new SqlConnection(configuration.GetConnectionString("SqlServerConnection")));

            services.AddScoped<IIdentityManager, IdentityManager>();
            services.AddScoped<IIdentityContext, IdentityContext>();
            services.AddDbContext<IIdentityContext, IdentityContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            });
            services.AddIdentity<BlogUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            }
            ).AddEntityFrameworkStores<IdentityContext>()
             .AddDefaultTokenProviders();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.IsJournalist, policy => policy.RequireClaim("BlogUserType", Enum.GetName(BlogUserType.JOURNALIST)));
            });

            return services;
        }

        public static IApplicationBuilder UseInfrastructureConfiguration(this WebApplication app)
        {
            using var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope();

            using var databaseContext = serviceScope.ServiceProvider.GetRequiredService<IDatabaseContext>();
            using var identityContext = serviceScope.ServiceProvider.GetRequiredService<IIdentityContext>();

            databaseContext.TestConnectionAsync().Wait();
            identityContext.TestConnectionAsync().Wait();

            if (databaseContext.AnyPendingMigrationsAsync().Result)
                databaseContext.MigrateAsync().Wait();

            if (identityContext.AnyPendingMigrationsAsync().Result)
                identityContext.MigrateAsync().Wait();

            return app;
        }
    }
}
