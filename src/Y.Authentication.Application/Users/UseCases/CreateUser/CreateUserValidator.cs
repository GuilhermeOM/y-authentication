using FluentValidation;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
public sealed class CreateUserValidator : AbstractValidator<CreateUserUseCase>
{
    public CreateUserValidator()
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

        RuleFor(x => x.Name).MaximumLength(50);

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .NotNull()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date must be in the past.");
    }
}
