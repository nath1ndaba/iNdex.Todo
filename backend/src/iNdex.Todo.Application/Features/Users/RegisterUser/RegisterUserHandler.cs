using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Users.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IHandler<RegisterUserRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result.Failure<CreatedResponse>(
                Error.Conflict("User.EmailConflict", $"A user with email '{request.Email}' already exists."));

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CreatedBy = "system"
        };

        await userRepository.AddAsync(user, cancellationToken);

        // Seed default settings
        var settings = new UserSettings
        {
            UserId = user.Id,
            CreatedBy = user.Id.ToString()
        };
        user.Settings.Add(settings);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedResponse(user.Id));
    }
}

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");
    }
}
