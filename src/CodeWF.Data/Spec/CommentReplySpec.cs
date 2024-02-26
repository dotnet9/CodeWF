namespace CodeWF.Data.Spec;

public class CommentReplySpec(Guid commentId) : BaseSpecification<CommentReplyEntity>(cr => cr.CommentId == commentId);