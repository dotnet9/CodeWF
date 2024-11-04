namespace BlogWebSite.Shared.RenderModes
{
    public class WasmRenderMode : IRenderMode
    {
        public string CurRenderModeName => nameof(IRenderMode.IsWasm);
    }
}
