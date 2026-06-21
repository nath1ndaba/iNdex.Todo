using FluentValidation;
using iNdex.Todo.Contracts.Requests;

namespace iNdex.Todo.Application.Features.TodoLists.CreateTodoList;

public sealed class CreateTodoListValidator : AbstractValidator<CreateTodoListRequest>
{
    public CreateTodoListValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("List name is required.")
            .MaximumLength(200).WithMessage("List name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color (e.g. #FF5733).")
            .When(x => x.Color is not null);

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
    }
}
