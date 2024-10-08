namespace CodeWF.Admin.Pages.Document;

[Route("/dms/updatelogs")]
public class DmUpdateLogList : DocumentList
{
    protected override ContentType Type => ContentType.UpdateLog;
}

[Route("/dms/documents")]
public class DmDocumentList : DocumentList
{
    protected override ContentType Type => ContentType.Document;
}