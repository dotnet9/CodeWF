using System.IO.Compression;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace CodeWF.Data.Exporting.Exporters;

public class ZippedJsonExporter<T>(IRepository<T> repository, string fileNamePrefix, string directory)
    : IExporter<T>
    where T : class
{
    public async Task<ExportResult> ExportData<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct)
    {
        IReadOnlyList<TResult> data = await repository.SelectAsync(selector, ct);
        ExportResult result = await ToZippedJsonResult(data, ct);
        return result;
    }

    private async Task<ExportResult> ToZippedJsonResult<TE>(IEnumerable<TE> list, CancellationToken ct)
    {
        string tempId = Guid.NewGuid().ToString();
        string exportDirectory = ExportManager.CreateExportDirectory(directory, tempId);
        foreach (TE item in list)
        {
            string json = JsonSerializer.Serialize(item, CodeWFJsonSerializerOptions.Default);
            await SaveJsonToDirectory(json, exportDirectory, $"{Guid.NewGuid()}.json", ct);
        }

        string distPath = Path.Join(directory, "export", $"{fileNamePrefix}-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.zip");
        ZipFile.CreateFromDirectory(exportDirectory, distPath);

        return new ExportResult { ExportFormat = ExportFormat.ZippedJsonFiles, FilePath = distPath };
    }

    private static async Task SaveJsonToDirectory(string json, string directory, string filename, CancellationToken ct)
    {
        string path = Path.Join(directory, filename);
        await File.WriteAllTextAsync(path, json, Encoding.UTF8, ct);
    }
}