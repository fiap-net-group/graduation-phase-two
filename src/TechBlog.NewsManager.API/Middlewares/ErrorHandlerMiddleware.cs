﻿using FluentValidation;
using Newtonsoft.Json;
using System.Net;
using TechBlog.NewsManager.API.Domain.Exceptions;
using TechBlog.NewsManager.API.Domain.Logger;
using TechBlog.NewsManager.API.Domain.Responses;

namespace PoliceDepartment.EvidenceManager.API.Middlewares
{
    public sealed class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public ErrorHandlerMiddleware(RequestDelegate next,
                                      ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized error caught by middleware - JWT TOKEN");

                    var code = HttpStatusCode.Unauthorized;

                    var result = JsonConvert.SerializeObject(new BaseResponse().AsError(ResponseMessage.UserIsNotAuthenticated));

                    await ErrorResponse(context, result, code);
                }
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning("Business error caught by middleware", ("exception", ex));

                await HandleExceptionAsync(context, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized error caught by middleware - API TOKEN", ("exception", ex));

                await HandleExceptionAsync(context, ex);
            }
            catch(ValidationException ex)
            {
                _logger.LogWarning("Validation error caught by middleware", ("exception", ex));

                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error caught by middleware", ex);

                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, BusinessException exception)
        {
            var code = HttpStatusCode.BadRequest;

            var result = JsonConvert.SerializeObject(new BaseResponse().AsError(null, exception.Message));

            return ErrorResponse(context, result, code);
        }

        private static Task HandleExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
        {
            var code = HttpStatusCode.Unauthorized;

            var result = JsonConvert.SerializeObject(new BaseResponse().AsError(null, exception.Message));

            return ErrorResponse(context, result, code);
        }

        private static Task HandleExceptionAsync(HttpContext context, ValidationException exception)
        {
            var code = HttpStatusCode.BadRequest;

            var result = JsonConvert.SerializeObject(new BaseResponse().AsError(ResponseMessage.InvalidInformation, exception.Errors.Select(e => e.ErrorMessage).ToArray()));

            return ErrorResponse(context, result, code);
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            var code = HttpStatusCode.InternalServerError;

            return ErrorResponse(context, code);
        }

        private static Task ErrorResponse(HttpContext context, HttpStatusCode code)
        {
            var result = JsonConvert.SerializeObject(new BaseResponse().AsError());

            return ErrorResponse(context, result, code);
        }

        private static Task ErrorResponse(HttpContext context, string result, HttpStatusCode code)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
