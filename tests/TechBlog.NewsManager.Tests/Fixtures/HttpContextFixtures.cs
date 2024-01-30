﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Claims;
using TechBlog.NewsManager.API.Domain.ValueObjects;

namespace TechBlog.NewsManager.Tests.Fixtures
{
    public class HttpContextFixtures
    {
        public HttpContext GenerateAuthenticateduserHttpContext(BlogUserType userType)
        {
            var identity = new ClaimsIdentity("authMethod");

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim("BlogUserType", Enum.GetName(userType)));

            var contextUser = new ClaimsPrincipal(identity);

            return new DefaultHttpContext()
            {
                User = contextUser
            };
        }

        public HttpContext GenerateDefaultHttpContext()
        {
            return new DefaultHttpContext();
        }

        public HttpContext GetResposeHttpContext(IResult response)
        {
            var context = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
                Response =
                {
                    Body = new MemoryStream(),
                },
            };

            response.ExecuteAsync(context).Wait();

            context.Response.Body.Position = 0;

            return context;
        }

        public async Task<T> GetObjectFromBodyAsync<T>(HttpContext context, string selectJson = null)
        {
            using var bodyReader = new StreamReader(context.Response.Body);

            string body = await bodyReader.ReadToEndAsync();

            if (selectJson is null)
                return JsonConvert.DeserializeObject<T>(body);

            var property = JObject.Parse(body)[selectJson];

            return (T)property.ToObject(typeof(T));
        }
    }
}
