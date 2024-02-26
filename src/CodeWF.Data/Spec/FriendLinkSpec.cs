namespace CodeWF.Data.Spec;

public class FriendLinkSpec(Guid id) : BaseSpecification<FriendLinkEntity>(f => f.Id == id);