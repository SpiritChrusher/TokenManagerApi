using FluentValidation;
using TokenManagerApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace TokenManagerApi.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    private readonly Models.TokenManagerDbContext _db;

    public UserDtoValidator(Models.TokenManagerDbContext db)
    {
        _db = db;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                return !await _db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email, cancellation);
            })
            .WithMessage("User already exists.");
    }
}
