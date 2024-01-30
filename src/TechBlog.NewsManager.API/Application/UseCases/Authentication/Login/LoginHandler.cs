﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TechBlog.NewsManager.API.Application.UseCases.BlogUsers.Create;
using TechBlog.NewsManager.API.Domain.Authentication;
using TechBlog.NewsManager.API.Domain.Extensions;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Domain.Responses;

namespace TechBlog.NewsManager.API.Application.UseCases.Authentication.Login
{
    public static class LoginHandler
    {
        public static string Route => "/api/v1/auth";
        public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
        public static Delegate Handle => Action;

        /// <summary>
        /// Generate the access token for the API
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="validator"></param>
        /// <param name="identityManager"></param>
        /// <param name="request">The login body</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <response code="200" cref="BaseResponseWithValue{T}">Authenticated successfully</response>
        /// <response code="400" cref="BaseResponse">User don't exists or Invalid credentials</response>
        public static async Task<IResult> Action(ILoggerManager logger, 
                                                 IValidator<LoginRequest> validator, 
                                                 IIdentityManager identityManager, 
                                                 LoginRequest request, 
                                                 CancellationToken cancellationToken)
        {
            logger.LogDebug("Begin Login", ("username", request.Username));

            validator.ThrowIfInvalid(request);

            var response = new BaseResponseWithValue<AccessTokenModel>();

            var user = await identityManager.GetByEmailAsync(request.Username, cancellationToken);

            if (!user.Exists)
            {
                logger.LogWarning("Officer don't exists", ("username", request.Username));
                    
                return Results.BadRequest(response.AsError(ResponseMessage.InvalidCredentials));
            }

            var accessToken = await identityManager.AuthenticateAsync(user, request.Password, cancellationToken,("name", user.Name));

            if (!accessToken.Valid)
            {
                logger.LogWarning("Invalid credentials", ("username", request.Username));

                return Results.BadRequest(response.AsError(ResponseMessage.InvalidCredentials));
            }

            logger.LogDebug("Success Login", ("username", request.Username));

            return Results.Ok(response.AsSuccess(accessToken));
        }
    }
}
