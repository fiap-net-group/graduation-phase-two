using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechBlog.NewsManager.API.Domain.Authentication
{
    public static class AuthorizationPolicies
    {
        public const string IsJournalist = nameof(IsJournalist);
    }
}