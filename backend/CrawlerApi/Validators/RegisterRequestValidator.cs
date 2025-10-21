using CrawlerApi.Models.Dtos.Auth;
using FluentValidation;

namespace CrawlerApi.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式无效")
            .MaximumLength(256).WithMessage("邮箱长度不能超过256个字符");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少3个字符")
            .MaximumLength(64).WithMessage("用户名不能超过64个字符")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("用户名只能包含字母、数字、下划线和连字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少6个字符")
            .MaximumLength(128).WithMessage("密码不能超过128个字符");

        RuleFor(x => x.DisplayName)
            .MaximumLength(128).WithMessage("显示名称不能超过128个字符")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}
