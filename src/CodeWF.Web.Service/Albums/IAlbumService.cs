namespace CodeWF.Web.Service.Albums;

public interface IAlbumService
{
    Task<List<AlbumBrief>> GetAlbumsAsync();
}