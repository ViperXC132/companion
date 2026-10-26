using System.Diagnostics;
using System.IO;

namespace DevAI.App;

/// <summary>Runs the separately installed OpenCode CLI in a user-selected project directory.</summary>
internal static class OpenCodeRunner
{
    public static async Task<string> RunAsync(string projectDirectory, string prompt, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(projectDirectory)) throw new DirectoryNotFoundException("The selected project folder no longer exists.");
        if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Enter a task for OpenCode.", nameof(prompt));

        var start = new ProcessStartInfo
        {
            FileName = "opencode",
            WorkingDirectory = Path.GetFullPath(projectDirectory),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        start.ArgumentList.Add("run");
        start.ArgumentList.Add(prompt.Trim());
        using var process = new Process { StartInfo = start };
        try
        {
            if (!process.Start()) throw new InvalidOperationException("OpenCode could not be started.");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new InvalidOperationException("OpenCode CLI was not found. Install OpenCode and ensure the opencode command is available on PATH.", ex);
        }

        using var registration = cancellationToken.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch { } });
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await stdout;
        var error = await stderr;
        var combined = string.Join(Environment.NewLine, new[] { output.Trim(), error.Trim() }.Where(x => x.Length > 0));
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"OpenCode exited with code {process.ExitCode}.{Environment.NewLine}{Limit(combined)}");
        return combined.Length == 0 ? "OpenCode finished without returning console output." : Limit(combined);
    }

    private static string Limit(string text) => text.Length <= 16000 ? text : text[..16000] + Environment.NewLine + "[Output truncated]";
}
