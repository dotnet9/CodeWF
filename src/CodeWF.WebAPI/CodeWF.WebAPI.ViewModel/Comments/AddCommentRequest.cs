﻿namespace CodeWF.WebAPI.ViewModel.Comments;

public record AddCommentRequest(string Url, Guid? ParentId, string UserName, string Email, string Content);