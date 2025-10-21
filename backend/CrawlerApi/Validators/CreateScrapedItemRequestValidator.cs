using CrawlerApi.Models.Dtos;
using FluentValidation;

namespace CrawlerApi.Validators;

public class CreateScrapedItemRequestValidator : AbstractValidator<CreateScrapedItemRequest>
{
    public CreateScrapedItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .MaximumLength(256).WithMessage("标题长度不能超过256个字符");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL不能为空")
            .Must(BeAValidUrl).WithMessage("URL格式无效")
            .MaximumLength(2048).WithMessage("URL长度不能超过2048个字符");

        RuleFor(x => x.Source)
            .MaximumLength(128).WithMessage("来源长度不能超过128个字符")
            .When(x => !string.IsNullOrEmpty(x.Source));

        RuleFor(x => x.Summary)
            .MaximumLength(1024).WithMessage("摘要长度不能超过1024个字符")
            .When(x => !string.IsNullOrEmpty(x.Summary));

        RuleFor(x => x.CollectedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("采集时间不能是未来时间")
            .When(x => x.CollectedAt.HasValue);
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
