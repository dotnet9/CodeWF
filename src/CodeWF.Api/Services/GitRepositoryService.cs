using System.Diagnostics;
using System.Text;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed class GitRepositoryService
{
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".json", ".jsonc", ".yaml", ".yml", ".xml", ".csv", ".ts", ".tsx", ".js", ".jsx",
        ".css", ".scss", ".less", ".html", ".htm", ".cs", ".csproj", ".sln", ".config", ".props", ".targets",
        ".razor", ".log", ".ini", ".env", ".bat", ".cmd"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".avif", ".ico", ".svg"
    };

    private readonly IOptionsMonitor<SiteOptions> siteOptions;

    public GitRepositoryService(IOptionsMonitor<SiteOptions> siteOptions)
    {
        this.siteOptions = siteOptions;
    }

    public Task<GitCommandResult> GetStatusAsync() => RunGitAsync("status", "--short", "--branch");

    public async Task<GitRepositoryStatusData> GetStatusDetailsAsync()
    {
        var status = await GetStatusAsync();
        if (!status.Success)
        {
            return new GitRepositoryStatusData(string.Empty, [], 0);
        }

        var branch = string.Empty;
        var changes = new List<GitChangeEntry>();
        foreach (var line in status.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                branch = line[3..].Trim();
                continue;
            }

            if (line.Length < 4)
            {
                continue;
            }

            var statusCode = line[..2].Trim();
            var path = line[3..].Trim();
            var arrowIndex = path.IndexOf(" -> ", StringComparison.Ordinal);
            var originalPath = arrowIndex >= 0 ? path[..arrowIndex] : null;
            var displayPath = arrowIndex >= 0 ? path[(arrowIndex + 4)..] : path;
            changes.Add(new GitChangeEntry(statusCode, displayPath, originalPath));
        }

        return new GitRepositoryStatusData(branch, changes, changes.Count);
    }

    public Task<GitCommandResult> GetLogAsync(int count) =>
        RunGitAsync("log", $"--max-count={Math.Clamp(count, 1, 100)}", "--date=iso", "--pretty=format:%h%x09%ad%x09%s");

    public Task<GitCommandResult> FetchAsync() => RunGitAsync("fetch", "--all", "--prune");

    public Task<GitCommandResult> PullAsync() => RunGitAsync("pull", "--ff-only");

    public async Task<GitCommandResult> CommitAsync(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new GitCommandResult(false, "git commit", string.Empty, "Commit message is required.", -1);
        }

        var add = await RunGitAsync("add", "-A");
        if (!add.Success)
        {
            return add;
        }

        return await RunGitAsync("commit", "-m", message.Trim());
    }

    public Task<GitFilePreviewResult> GetFilePreviewAsync(string relativePath)
    {
        var root = RepoRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath ?? string.Empty));
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult(new GitFilePreviewResult(false, relativePath ?? string.Empty, Path.GetFileName(relativePath), "missing"));
        }

        return Task.FromResult(CreateFilePreview(fullPath, relativePath));
    }

    private async Task<GitCommandResult> RunGitAsync(params string[] arguments)
    {
        var root = RepoRoot();
        if (!Directory.Exists(root))
        {
            return new GitCommandResult(false, $"git {string.Join(' ', arguments)}", string.Empty, $"Assets directory does not exist: {root}", -1);
        }

        var command = $"git {string.Join(' ', arguments)}";
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await process.WaitForExitAsync(cts.Token);
            var output = await outputTask;
            var error = await errorTask;
            return new GitCommandResult(process.ExitCode == 0, command, output, error, process.ExitCode);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore process shutdown errors.
            }

            return new GitCommandResult(false, command, string.Empty, "Git command timed out.", -1);
        }
        catch (Exception ex)
        {
            return new GitCommandResult(false, command, string.Empty, ex.Message, -1);
        }
    }

    private GitFilePreviewResult CreateFilePreview(string fullPath, string relativePath)
    {
        var fileInfo = new FileInfo(fullPath);
        var extension = Path.GetExtension(fullPath);
        var name = Path.GetFileName(fullPath);

        if (ImageExtensions.Contains(extension))
        {
            var mimeType = extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".avif" => "image/avif",
                ".ico" => "image/x-icon",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };

            var bytes = File.ReadAllBytes(fullPath);
            return new GitFilePreviewResult(true, relativePath, name, "image", null, $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}", mimeType, fileInfo.Length);
        }

        if (TextExtensions.Contains(extension))
        {
            var text = File.ReadAllText(fullPath, Encoding.UTF8);
            return new GitFilePreviewResult(true, relativePath, name, "text", text, null, "text/plain; charset=utf-8", fileInfo.Length);
        }

        return new GitFilePreviewResult(true, relativePath, name, "binary", null, null, "application/octet-stream", fileInfo.Length);
    }

    private string RepoRoot()
    {
        var configured = siteOptions.CurrentValue.LocalAssetsDir;
        var expanded = Environment.ExpandEnvironmentVariables(configured);
        return Path.GetFullPath(expanded);
    }
}
