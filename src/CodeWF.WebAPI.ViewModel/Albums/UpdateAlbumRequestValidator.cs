﻿namespace CodeWF.WebAPI.ViewModel.Albums;

public class UpdateAlbumRequestValidator : AbstractValidator<UpdateAlbumRequest>
{
    public UpdateAlbumRequestValidator()
    {
        RuleFor(x => x.Name).NotNull().Length(AlbumConsts.MinNameLength, AlbumConsts.MaxNameLength)
            .WithMessage($"名称长度范围[{AlbumConsts.MinNameLength},{AlbumConsts.MaxNameLength}]");

        RuleFor(x => x.Slug).NotNull().Length(AlbumConsts.MinSlugLength, AlbumConsts.MaxSlugLength)
            .WithMessage($"别名长度范围[{AlbumConsts.MinSlugLength},{AlbumConsts.MaxSlugLength}]");

        RuleFor(x => x.Cover).NotNull().Length(AlbumConsts.MinCoverLength, AlbumConsts.MaxCoverLength)
            .WithMessage($"封面URL长度范围[{AlbumConsts.MinCoverLength},{AlbumConsts.MaxCoverLength}]");

        RuleFor(x => x.Description).MaximumLength(AlbumConsts.MaxDescriptionLength)
            .WithMessage($"描述长度不能大于{AlbumConsts.MaxDescriptionLength}]");
    }
}