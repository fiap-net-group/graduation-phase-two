using AutoMapper;
using TechBlog.NewsManager.API.Application.ViewModels;
using TechBlog.NewsManager.API.Domain.Database;
using TechBlog.NewsManager.API.Domain.Entities;
using TechBlog.NewsManager.API.Domain.Exceptions;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Domain.Strategies.GetBlogNews;

namespace TechBlog.NewsManager.API.Application.Strategies.GetBlogNewStrategy
{
    public class GetByNameStrategy : IGetBlogNewsStrategy
    {
        private readonly ILoggerManager _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBlogNewsStrategy Strategy => GetBlogNewsStrategy.GET_BY_NAME;

        public GetByNameStrategy(ILoggerManager logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<object> RunAsync(GetBlogNewsStrategyBody body, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting blognew by name", ("strategy", Strategy), ("body", body));

            if (body is null || !body.ValidName)
            {
                _logger.LogInformation("Invalid body", ("strategy", Strategy), ("body", body));

                throw new BusinessException("Invalid strategy body");
            }

            var blogNews = (await _unitOfWork.BlogNew.GetByNameAsync(body.Name, cancellationToken));

            _logger.LogDebug("End getting blognew by name", ("strategy", Strategy), ("body", body), ("newsFoundCount", blogNews.Count()));

            return _mapper.Map<IEnumerable<BlogNewViewModel>>(blogNews);
        }
    }
}
