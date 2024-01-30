using FluentValidation;
using TechBlog.NewsManager.API.Domain.Exceptions;

namespace TechBlog.NewsManager.API.Domain.Extensions
{
    public static class ValidatorExtensions
    {
        public static void ThrowIfInvalid<T>(this IValidator<T> validator, T model)
        {
            validator.Validate(model, options =>
            {
                options.ThrowOnFailures();
            });
        }
    }
}
