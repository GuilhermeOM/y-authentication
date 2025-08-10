using FluentValidation;

namespace Y.Authentication.Application.Users.UseCases.LoginUser;
public sealed class LoginUserValidator : AbstractValidator<LoginUserUseCase>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .NotNull()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}
