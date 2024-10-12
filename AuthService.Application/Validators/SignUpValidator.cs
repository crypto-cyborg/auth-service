﻿using AuthService.Application.Data.Dtos;
using FluentValidation;

namespace AuthService.Application.Validators
{
    public class SignUpValidator : AbstractValidator<SignUpDTO>
    {
        public SignUpValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .Matches(RegexPatterns.Username)
                .WithMessage(
                    "Username must be between 8 and 24 characters long and can only contain letters and numbers"
                );

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .Matches(RegexPatterns.Password)
                .WithMessage(
                    "Password must be between 8 and 24 characters long and can only contain letters, numbers, and symbols !@#~*."
                );

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .Matches(RegexPatterns.Email)
                .WithMessage("Email doesn't match the pattern");

            RuleFor(x => x.FirstName).NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("Surname is required");
        }
    }
}
