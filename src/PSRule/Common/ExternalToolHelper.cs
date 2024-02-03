// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;

namespace PSRule;

internal sealed class ExternalTool : IDisposable
{
    private Process _Process;
    private readonly StringBuilder _Output;
    private readonly StringBuilder _Error;
    private readonly AutoResetEvent _ErrorWait;
    private readonly AutoResetEvent _OutputWait;
    private readonly int _Interval;
    private readonly int _Timeout;
    private readonly string _BinaryPath;
    private bool _Disposed;

    private ExternalTool(string binaryPath, int timeout, string version = null)
    {
        _Output = new StringBuilder();
        _Error = new StringBuilder();
        _Interval = 1000;
        _Timeout = timeout;
        _BinaryPath = binaryPath;

        _ErrorWait = new AutoResetEvent(false);
        _OutputWait = new AutoResetEvent(false);
    }

    public bool HasExited => _Process.HasExited;

    internal static ExternalTool Get(string defaultPath, string binary)
    {
        if (!TryPathFromDefault(defaultPath, binary, out var binaryPath) && !TryPathFromEnvironment(binary, out binaryPath))
            return null;

        return new ExternalTool(binaryPath, 0, null);
    }

    private static bool TryPathFromDefault(string defaultPath, string binary, out string binaryPath)
    {
        return TryPath(binary, defaultPath, out binaryPath);
    }

    public bool WaitForExit(string args, out int exitCode)
    {
        var startInfo = new ProcessStartInfo(_BinaryPath, args)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Environment.GetWorkingPath(),
        };
        _Process = Process.Start(startInfo);
        _Process.ErrorDataReceived += Bicep_ErrorDataReceived;
        _Process.OutputDataReceived += Bicep_OutputDataReceived;

        _Process.BeginErrorReadLine();
        _Process.BeginOutputReadLine();

        _ErrorWait.Reset();
        _OutputWait.Reset();

        if (!_Process.HasExited)
        {
            var timeoutCount = 0;
            while (!_Process.WaitForExit(_Interval) && !_Process.HasExited && timeoutCount < _Timeout)
                timeoutCount++;
        }

        exitCode = _Process.HasExited ? _Process.ExitCode : -1;
        return _Process.HasExited && _ErrorWait.WaitOne(_Interval) && _OutputWait.WaitOne();
    }

    public string GetOutput()
    {
        return _Output.ToString();
    }

    public string GetError()
    {
        return _Error.ToString();
    }

    private void Bicep_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            _OutputWait.Set();
        }
        else
        {
            _Output.AppendLine(e.Data);
        }
    }

    private void Bicep_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            _ErrorWait.Set();
        }
        else
        {
            var errors = GetErrorLine(e.Data);
            for (var i = 0; i < errors.Length; i++)
                _Error.AppendLine(errors[i]);
        }
    }

    private static string[] GetErrorLine(string input)
    {
        var lines = input.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();
        for (var i = 0; i < lines.Length; i++)
            if (!lines[i].Contains(": Warning ") && !lines[i].Contains(": Info "))
                result.Add(lines[i]);

        return result.ToArray();
    }

    private static bool TryPathFromEnvironment(string binary, out string binaryPath)
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

    private static bool TryPath(string binary, string path, out string binaryPath)
    {
        binaryPath = null;
        if (string.IsNullOrEmpty(path))
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
                _Process.Dispose();
            }
            _Error.Clear();
            _Output.Clear();
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
