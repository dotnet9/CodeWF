using Quartz;

namespace WebSite.Jobs;

public class FileCleanerJob(IWebHostEnvironment env) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var directoryPath = Path.Combine(env.WebRootPath, "IconFolder");
        try
        {
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                var creationTime = File.GetCreationTime(file);
                if ((DateTime.Now - creationTime).TotalMinutes > 2)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log them
            Console.WriteLine($"Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}