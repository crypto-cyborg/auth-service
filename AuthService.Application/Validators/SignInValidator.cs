using AuthService.Application.Data.Dtos;
using FluentValidation;

namespace AuthService.Application.Validators
{
    public class SignInValidator : AbstractValidator<SignInDTO>
    {
        public SignInValidator()
        {
            RuleFor(u => u.Username).NotEmpty().WithMessage("Username is required");

            RuleFor(u => u.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
