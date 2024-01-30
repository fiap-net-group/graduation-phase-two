using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TechBlog.NewsManager.API.Domain.Database;
using TechBlog.NewsManager.API.Domain.Exceptions;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Domain.Responses;

namespace TechBlog.NewsManager.API.Application.UseCases.BlogNews.Update
{
    public class UpdateBlogNewHandler
    {
        public static string Route => "/api/v1/blognew/{id:guid}";
        public static string[] Methods => new string[] { HttpMethod.Put.ToString() };   

        public static Delegate Handle => Action;

        /// <summary>
        /// The action that updates a blog new.
        /// </summary>
        /// <param name="logger">The logger manager.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="user">The user making the request.</param>
        /// <param name="id">The ID of the blog new to update.</param>
        /// <param name="request">The request with the new data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200" cref="BaseResponse">Success</response>
        /// <response code="400" cref="BaseResponse">When the request is invalid</response>
        /// <response code="403" cref="BaseResponse">When the user is not allowed to update the blog new</response>
        /// <response code="404" cref="BaseResponse">When the blog new was not found</response>
        /// <returns>The result of the update blog new action.</returns>
        /// <exception cref="InfrastructureException">An unexpected error ocurred</exception>
        /// <exception cref="ValidationException">When the request is invalid</exception>
        
        public static async Task<IResult> Action(ILoggerManager logger,
                                                    IUnitOfWork unitOfWork,
                                                    ClaimsPrincipal user,
                                                    Guid id,
                                                    UpdateBlogNewRequest request,
                                                    CancellationToken cancellationToken)
        {
            var response = new BaseResponse();

            var blogNew = await unitOfWork.BlogNew.GetByIdAsync(id, cancellationToken);

            if (!blogNew.Enabled)
            {
                logger.LogDebug("Blog new not found", ("Id", id));

                return Results.NotFound(response.AsError(ResponseMessage.BlogNewNotFound));
            }

            if (blogNew.AuthorId != user.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                logger.LogInformation("User not allowed to update blog new", ("Id", id), ("UserId", user.FindFirstValue(ClaimTypes.NameIdentifier)), ("BlogNewAuthorId", blogNew.AuthorId));

                return Results.Forbid();
            }

            try
            {
                blogNew.Update(request.Title, request.Description, request.Body);

                await unitOfWork.BlogNew.UpdateAsync(blogNew, cancellationToken);
            }
            catch (ValidationException ex)
            {
                logger.LogDebug("Invalid request", ("Id", id), ("Request", request), ("Errors", ex.Value));

                return Results.BadRequest(response.AsError(ResponseMessage.InvalidBody));
            }
            catch
            {
                logger.LogError("An error ocurred at the database", default, ("Id", id));

                throw new InfrastructureException("An unexpected error ocurred");
            }

            logger.LogDebug("Blog new updated", ("Id", id), ("Request", request));

            return Results.Ok(response);
        }
    }
}