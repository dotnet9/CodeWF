using System.Diagnostics;
using System.Text;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed class GitRepositoryService
{
    private readonly IOptionsMonitor<SiteOptions> siteOptions;

    public GitRepositoryService(IOptionsMonitor<SiteOptions> siteOptions)
    {
        this.siteOptions = siteOptions;
    }

    public Task<GitCommandResult> GetStatusAsync() => RunGitAsync("status", "--short", "--branch");

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

    private async Task<GitCommandResult> RunGitAsync(params string[] arguments)
    {
        var root = Path.GetFullPath(Environment.ExpandEnvironmentVariables(siteOptions.CurrentValue.LocalAssetsDir));
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
}
