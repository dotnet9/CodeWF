namespace BlogWebSite.Client.RenderModes
{
    public class WasmRenderMode : IRenderMode
    {
        public string CurRenderModeName => nameof(IRenderMode.IsWasm);
    }
}
