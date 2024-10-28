using AuthService.Application.Data.Dtos;
using FluentValidation;

namespace AuthService.Application.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Password cannot be empty");

        RuleFor(x => x.CofirmNewPassword)
            .NotEmpty()
            .WithMessage("Password cannot be empty")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }
}
