namespace CodeWF.WebAPI.ViewModel.Albums;

public record GetAlbumListRequest(string? Keywords, int Current, int PageSize);