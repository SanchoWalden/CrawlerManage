using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CrawlerApi.Extensions;

public static class ValidationExtensions
{
    public static async Task<Results<Ok<TResult>, ValidationProblem>> ValidateAndExecuteAsync<TRequest, TResult>(
        this IValidator<TRequest> validator,
        TRequest request,
        Func<Task<TResult>> onSuccess)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return TypedResults.ValidationProblem(errors);
        }

        var result = await onSuccess();
        return TypedResults.Ok(result);
    }
}
