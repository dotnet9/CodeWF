namespace CodeWF.WebAPI.Infrastructure.Comments;

internal class CommentConfig : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Comments", CodeWFConsts.DbSchema);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(CommentConsts.MaxUrlLength);
        builder.Property(x => x.UserName).IsRequired().HasMaxLength(CommentConsts.MaxUserNameLength);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(CommentConsts.MaxEmailLength);
        builder.Property(x => x.Content).HasMaxLength(CommentConsts.MaxContentLength);
        builder.Property(x => x.ParentId);
        builder.Property(x => x.Visible);
    }
}