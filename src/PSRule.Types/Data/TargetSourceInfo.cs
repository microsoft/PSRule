// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Expressions;

namespace PSRule.Data;

/// <summary>
/// An object source location reported by a downstream tool.
/// </summary>
public sealed class TargetSourceInfo : IEquatable<TargetSourceInfo>, IFileInfo
{
    private const string PROPERTY_FILE = "file";
    private const string PROPERTY_LINE = "line";
    private const string PROPERTY_POSITION = "position";
    private const string PROPERTY_TYPE = "type";

    private const string COLON = ":";
    private const string COLON_SPACE = ": ";

    /// <summary>
    /// Creates an empty source information structure.
    /// </summary>
    public TargetSourceInfo()
    {
        // Do nothing
    }

    internal TargetSourceInfo(InputFileInfo info)
    {
        File = info.FullName;
        Type = "File";
    }

    internal TargetSourceInfo(FileInfo info)
    {
        File = info.FullName;
        Type = "File";
    }

    internal TargetSourceInfo(Uri uri)
    {
        File = uri.AbsoluteUri;
        Type = "File";
    }

    /// <summary>
    /// The file path of the source file.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_FILE)]
    public string? File { get; internal set; }

    /// <summary>
    /// The first line of the object.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_LINE)]
    public int? Line { get; internal set; }

    /// <summary>
    /// The first position of the object.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_POSITION)]
    public int? Position { get; internal set; }

    /// <summary>
    /// The type of source.
    /// </summary>
    [JsonProperty(PropertyName = PROPERTY_TYPE)]
    public string? Type { get; internal set; }

    string? IFileInfo.Path => File;

    string? IFileInfo.Extension => Path.GetExtension(File);

    /// <inheritdoc/>
    IFileStream IFileInfo.GetFileStream()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Equals(TargetSourceInfo other)
    {
        return other != null &&
            File == other.File &&
            Line == other.Line &&
            Position == other.Position &&
            Type == other.Type;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is TargetSourceInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (File != null ? File.GetHashCode() : 0);
            hash = hash * 23 + (Line.HasValue ? Line.Value.GetHashCode() : 0);
            hash = hash * 23 + (Position.HasValue ? Position.Value.GetHashCode() : 0);
            hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToString(null, false);
    }

    /// <summary>
    /// Converts the souce information into a formatted string for display.
    /// </summary>
    /// <param name="defaultType">The default type to use if the type was not specified.</param>
    /// <param name="useRelativePath">Determine if a relative path is returned.</param>
    /// <returns>A formatted source string.</returns>
    public string ToString(string? defaultType, bool useRelativePath)
    {
        var type = Type ?? defaultType;
        var file = GetPath(useRelativePath);
        return string.IsNullOrEmpty(type)
            ? string.Concat(file, COLON, Line, COLON, Position)
            : string.Concat(type, COLON_SPACE, file, COLON, Line, COLON, Position);
    }

    internal string? GetPath(bool useRelativePath)
    {
        return useRelativePath ? PathHelpers.NormalizePath(Environment.GetWorkingPath(), File) : File;
    }
}
