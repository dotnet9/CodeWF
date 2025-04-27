namespace CodeWF.Models;

public record PageData<T>(int PageIndex, int PageSize, int Total, List<T> Data);