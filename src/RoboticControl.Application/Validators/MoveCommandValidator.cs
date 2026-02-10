using FluentValidation;
using RoboticControl.Application.DTOs;

namespace RoboticControl.Application.Validators;

/// <summary>
/// Validator for move commands with work envelope checks
/// </summary>
public class MoveCommandValidator : AbstractValidator<MoveCommandDto>
{
    public MoveCommandValidator()
    {
        RuleFor(x => x.X)
            .InclusiveBetween(-1000, 1000)
            .WithMessage("X coordinate must be between -1000 and 1000 mm");

        RuleFor(x => x.Y)
            .InclusiveBetween(-1000, 1000)
            .WithMessage("Y coordinate must be between -1000 and 1000 mm");

        RuleFor(x => x.Z)
            .InclusiveBetween(0, 2000)
            .WithMessage("Z coordinate must be between 0 and 2000 mm");

        When(x => x.RotationX.HasValue, () =>
        {
            RuleFor(x => x.RotationX!.Value)
                .InclusiveBetween(-180, 180)
                .WithMessage("Rotation X must be between -180 and 180 degrees");
        });

        When(x => x.RotationY.HasValue, () =>
        {
            RuleFor(x => x.RotationY!.Value)
                .InclusiveBetween(-180, 180)
                .WithMessage("Rotation Y must be between -180 and 180 degrees");
        });

        When(x => x.RotationZ.HasValue, () =>
        {
            RuleFor(x => x.RotationZ!.Value)
                .InclusiveBetween(-180, 180)
                .WithMessage("Rotation Z must be between -180 and 180 degrees");
        });
    }
}
