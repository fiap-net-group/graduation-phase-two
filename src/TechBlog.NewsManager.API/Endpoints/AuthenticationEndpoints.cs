using TechBlog.NewsManager.API.Application.UseCases.Authentication.Login;
using TechBlog.NewsManager.API.Application.UseCases.BlogUsers.Create;
using TechBlog.NewsManager.API.Application.ViewModels;
using TechBlog.NewsManager.API.Domain.Authentication;
using TechBlog.NewsManager.API.Domain.Responses;

namespace TechBlog.NewsManager.API.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static IApplicationBuilder MapAuthenticationEndpoints(this WebApplication app)
        {
            app.MapMethods(LoginHandler.Route, LoginHandler.Methods, LoginHandler.Handle)
             .WithTags("Auth")
             .WithDescription("Authenticate the user")
             .WithDisplayName("Authenticate the user")
             .ProducesValidationProblem()
             .Produces<BaseResponseWithValue<AccessTokenModel>>(StatusCodes.Status200OK)
             .Produces<BaseResponse>(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
