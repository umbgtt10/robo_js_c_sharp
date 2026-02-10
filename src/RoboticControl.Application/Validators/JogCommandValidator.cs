using FluentValidation;
using RoboticControl.Application.DTOs;

namespace RoboticControl.Application.Validators;

/// <summary>
/// Validator for jog (relative movement) commands
/// </summary>
public class JogCommandValidator : AbstractValidator<JogCommandDto>
{
    public JogCommandValidator()
    {
        RuleFor(x => x.DeltaX)
            .InclusiveBetween(-100, 100)
            .WithMessage("Delta X must be between -100 and 100 mm");

        RuleFor(x => x.DeltaY)
            .InclusiveBetween(-100, 100)
            .WithMessage("Delta Y must be between -100 and 100 mm");

        RuleFor(x => x.DeltaZ)
            .InclusiveBetween(-100, 100)
            .WithMessage("Delta Z must be between -100 and 100 mm");

        // At least one delta must be non-zero
        RuleFor(x => x)
            .Must(x => Math.Abs(x.DeltaX) > 0.01 || Math.Abs(x.DeltaY) > 0.01 || Math.Abs(x.DeltaZ) > 0.01)
            .WithMessage("At least one delta value must be non-zero");
    }
}
