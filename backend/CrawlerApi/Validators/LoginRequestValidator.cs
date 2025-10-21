using CrawlerApi.Models.Dtos.Auth;
using FluentValidation;

namespace CrawlerApi.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty().WithMessage("邮箱或用户名不能为空")
            .MaximumLength(256).WithMessage("输入长度不能超过256个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MaximumLength(128).WithMessage("密码长度不能超过128个字符");
    }
}
