using Dapper;
using System.Data.SqlClient;
using TechBlog.NewsManager.API.Domain.Database;
using TechBlog.NewsManager.API.Domain.Entities;
using TechBlog.NewsManager.API.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace TechBlog.NewsManager.API.Infrastructure.Database.Repositories
{
    public sealed class BlogNewsRepository : IBlogNewsRepository
    {
        private readonly IDatabaseContext _context;
        private readonly SqlConnection _databaseConnection;

        private readonly int _timeout;

        public BlogNewsRepository(IDatabaseContext context, SqlConnection databaseConnection, IConfiguration configuration)
        {
            _context = context;
            _databaseConnection = databaseConnection;

            _timeout = configuration.GetValue<int>("DatabaseTimeoutInSeconds");
        }

        public async Task AddAsync(BlogNew blogNew, CancellationToken cancellationToken = default)
        {
            await _context.BlogNew.AddAsync(blogNew, cancellationToken);
        }

        public async Task<IEnumerable<BlogNew>> GetByCreatedDateAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var blogNews = await GetFromDatabase(BlogNewsQueriesExtensions.GetByCreateDateIntervalQuery, new { from, to }, cancellationToken);

            return blogNews ?? Enumerable.Empty<BlogNew>();
        }

        public async Task<IEnumerable<BlogNew>> GetByCreateOrUpdateDateAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var blogNews = await GetFromDatabase(BlogNewsQueriesExtensions.GetByCreateOrUpdateDateIntervalQuery, new { from, to }, cancellationToken);

            return blogNews ?? Enumerable.Empty<BlogNew>();
        }

        public async Task<BlogNew> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var blogNew = await _context.BlogNew.AsNoTracking()
                                                .Include(b => b.Author)
                                                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            blogNew ??= new BlogNew { Enabled = false };

            blogNew?.Author?.WrittenNews?.Clear();

            return blogNew;
        }

        public async Task<IEnumerable<BlogNew>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var blogNews = await GetFromDatabase(BlogNewsQueriesExtensions.GetByNameQuery, new { name }, cancellationToken);

            return blogNews ?? Enumerable.Empty<BlogNew>();
        }

        public async Task<IEnumerable<BlogNew>> GetByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new StringBuilder($"WHERE News.[Tags] LIKE '%{tags[0]}%'");

            for (int i = 1; i < tags.Length; i++)
            {
                queryBuilder.Append($" OR News.[Tags] LIKE '%{tags[0]}%'");
            }

            var query = BlogNewsQueriesExtensions.GetByTagsQuery + queryBuilder.ToString();

            var blogNews = await GetFromDatabase(query, cancellationToken: cancellationToken);

            return blogNews ?? Enumerable.Empty<BlogNew>();
        }

        private async Task<IEnumerable<BlogNew>> GetFromDatabase(string sql, object parameters = default, CancellationToken cancellationToken = default)
        {
            await _databaseConnection.OpenAsync(cancellationToken);

            var blogNews = new List<BlogNew>();

            await _databaseConnection.QueryAsync<BlogNew, BlogUser, BlogNew>
                 (sql,
                 (blogNew, blogUser) =>
                 {
                     if (blogNews.Exists(r => r.Id == blogNew.Id))
                         return blogNew;

                     blogNew.Author = blogUser;

                     blogNews.Add(blogNew);

                     return blogNew;
                 },
                 splitOn: "Id,Id",
                 param: parameters,
                 commandTimeout: _timeout);

            await _databaseConnection.CloseAsync();

            return blogNews;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _context.BlogNew.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task UpdateAsync(BlogNew updatedBlogNew, CancellationToken cancellationToken = default)
        {
            await _context.BlogNew
                .Where(x => x.Id == updatedBlogNew.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(s => s.Title, updatedBlogNew.Title)
                                            .SetProperty(s => s.Body, updatedBlogNew.Body)
                                            .SetProperty(s => s.Description, updatedBlogNew.Description)
                                            .SetProperty(s => s.LastUpdateAt, DateTime.Now), cancellationToken);
        }
    }
}
