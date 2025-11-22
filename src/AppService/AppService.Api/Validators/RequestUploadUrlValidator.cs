using AppService.Api.Models.Requests;
using FluentValidation;

namespace AppService.Api.Validators;


public class RequestUploadUrlValidator : AbstractValidator<RequestUploadUrlRequest>
{
    public RequestUploadUrlValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("File size cannot exceed 10MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(ct => ct.StartsWith("image/")).WithMessage("Only image files are allowed");
    }
}