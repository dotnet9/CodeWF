using FluentValidation;
using WebSite.ViewModels;

namespace WebSite.FluentValidations;

public class ConvertIconRequestValidator : AbstractValidator<ConvertIconRequest>
{
    public ConvertIconRequestValidator()
    {
        RuleFor(x => x.SourceImage)
            .NotNull().WithMessage("源图像文件不能为空")
            .Must(x => x.Length is > 0 and <= 10 * 1024 * 1024).WithMessage("文件大小必须在0到10MB之间");

        RuleFor(x => x.ConvertSizes)
            .NotNull().WithMessage("转换大小数组不能为空")
            .Must(x => x.Length > 0).WithMessage("转换大小数组必须包含至少一个元素");
    }
}