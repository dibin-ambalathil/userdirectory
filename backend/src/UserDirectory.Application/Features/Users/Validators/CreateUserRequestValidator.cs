using FluentValidation;
using UserDirectory.Application.Features.Users.DTOs;

namespace UserDirectory.Application.Features.Users.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100);

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120);

        RuleFor(x => x.City)
            .NotEmpty();

        RuleFor(x => x.State)
            .NotEmpty();

        RuleFor(x => x.Pincode)
            .NotEmpty()
            .Length(4, 10);
    }
}
