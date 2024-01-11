﻿namespace CodeWF.WebAPI.ViewModel.ActionLogs;

public record GetActionLogListRequest(string? Keywords,
    DateTime? StartCreationTime, DateTime? EndCreationTime, int Current, int PageSize, string? Sort);