﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechBlog.NewsManager.API.Domain.Entities;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Infrastructure.Database.Context;

namespace TechBlog.NewsManager.API.Infrastructure.Authentication.Configuration.Context
{
    public class IdentityContext : IdentityDbContext<BlogUser>, IIdentityContext
    {
        private readonly ILoggerManager _logger;

        public IdentityContext(DbContextOptions<IdentityContext> options, ILoggerManager logger) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(IdentityContext).Assembly);

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(builder);
        }

        public async Task<bool> AnyPendingMigrationsAsync()
        {
            try
            {
                var migrations = await Database.GetPendingMigrationsAsync();

                return migrations.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task MigrateAsync()
        {
            await Database.MigrateAsync();
        }

        public async Task TestConnectionAsync()
        {
            try
            {
                _ = await Database.ExecuteSqlRawAsync("SELECT 1");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Invalid database connection", ex, ("context", nameof(IdentityContext)));
                Environment.Exit(2);
            }
        }
    }
}
