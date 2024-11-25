namespace BlogWebSite.Shared.RenderModes
{
    public interface IRenderMode
    {
        string CurRenderModeName { get; }
        virtual bool IsWasm() => CurRenderModeName is nameof(IsWasm);
    }
}
