namespace CodeWF.WebAPI.ViewModel.Albums;

public record GetAlbumListResponse(IEnumerable<AlbumDto>? Data, long Total, bool Success, int PageSize, int Current);