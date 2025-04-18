// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;

namespace PSRule;

internal sealed class ExternalTool : IDisposable
{
    private Process? _Process;
    private readonly StringBuilder _Output;
    private readonly StringBuilder _Error;
    private readonly AutoResetEvent _ErrorWait;
    private readonly AutoResetEvent _OutputWait;
    private readonly int _Interval;
    private readonly string _BinaryPath;
    private bool _Disposed;

    private ExternalTool(string binaryPath)
    {
        _Output = new StringBuilder();
        _Error = new StringBuilder();
        _Interval = 1000;
        _BinaryPath = binaryPath;
        _ErrorWait = new AutoResetEvent(false);
        _OutputWait = new AutoResetEvent(false);
    }

    public bool HasExited => _Process?.HasExited ?? false;

    internal static ExternalTool? Get(string? defaultPath, string binary)
    {
        if (!TryPathFromDefault(defaultPath, binary, out var binaryPath) && !TryPathFromEnvironment(binary, out binaryPath))
            return null;

        return binaryPath == null ? null : new ExternalTool(binaryPath);
    }

    private static bool TryPathFromDefault(string? defaultPath, string binary, out string? binaryPath)
    {
        return TryPath(binary, defaultPath, out binaryPath);
    }

    public bool WaitForExit(string args, out int exitCode)
    {
        _Process = new Process
        {
            StartInfo = new ProcessStartInfo(_BinaryPath, args)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Environment.GetWorkingPath(),
            }
        };
        _Process.ErrorDataReceived += Tool_ErrorDataReceived;
        _Process.OutputDataReceived += Tool_OutputDataReceived;

        _ErrorWait.Reset();
        _OutputWait.Reset();

        _Process.Start();
        _Process.BeginErrorReadLine();
        _Process.BeginOutputReadLine();

        _Process.WaitForExit();

        exitCode = _Process.HasExited ? _Process.ExitCode : -1;
        return _Process.HasExited && _ErrorWait.WaitOne(_Interval) && _OutputWait.WaitOne();
    }

    public string GetOutput()
    {
        lock (_Output)
        {
            return _Output.ToString();
        }
    }

    public string GetError()
    {
        lock (_Error)
        {
            return _Error.ToString();
        }
    }

    private void Tool_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            _OutputWait.Set();
        }
        else
        {
            lock (_Output)
            {
                _Output.AppendLine(e.Data);
            }
        }
    }

    private void Tool_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            _ErrorWait.Set();
        }
        else
        {
            var errors = GetErrorLine(e.Data);
            if (errors.Length == 0)
                return;

            lock (_Error)
            {
                for (var i = 0; i < errors.Length; i++)
                    _Error.AppendLine(errors[i]);
            }
        }
    }

    private static string[] GetErrorLine(string input)
    {
        var lines = input.Split([System.Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();
        for (var i = 0; i < lines.Length; i++)
            if (!lines[i].Contains(": Warning ") && !lines[i].Contains(": Info "))
                result.Add(lines[i]);

        return [.. result];
    }

    private static bool TryPathFromEnvironment(string binary, out string? binaryPath)
    {
        binaryPath = null;
        if (!Environment.TryPathEnvironmentVariable(out var path))
            return false;

        for (var i = 0; path != null && i < path.Length; i++)
            if (TryPath(binary, path[i], out binaryPath))
                return true;

        binaryPath = null;
        return false;
    }

    private static bool TryPath(string binary, string? path, out string? binaryPath)
    {
        binaryPath = null;
        if (path == null || string.IsNullOrEmpty(path))
            return false;

        binaryPath = Path.Combine(path, binary);
        if (File.Exists(binaryPath))
            return true;

        binaryPath = null;
        return false;
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _ErrorWait.Dispose();
                _OutputWait.Dispose();
                _Process?.Dispose();
            }
            lock (_Error)
            {
                _Error.Clear();
            }
            lock (_Output)
            {
                _Output.Clear();
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
