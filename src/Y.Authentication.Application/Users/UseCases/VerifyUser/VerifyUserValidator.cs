using FluentValidation;

namespace Y.Authentication.Application.Users.UseCases.VerifyUser;
internal class VerifyUserValidator : AbstractValidator<VerifyUserUseCase>
{
    public VerifyUserValidator()
    {
        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .NotNull();
    }
}
