using System;

namespace UnityCli.Lsp.Core;

public static class LspPathUtilities
{
    public static string NormalizeRelative(string relative) =>
        (relative ?? string.Empty).Replace('\\', '/');

    public static string Path2Uri(string path) =>
        "file://" + path.Replace('\\', '/');

    public static string ToRel(string fullPath, string root)
    {
        var normFull = NormalizeRelative(fullPath);
        var normRoot = NormalizeRelative(root).TrimEnd('/');
        if (normFull.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase))
        {
            return normFull.Substring(normRoot.Length + 1);
        }

        return normFull;
    }

    public static string NormalizeNamePath(string raw) =>
        string.Join(
            '/',
            (raw ?? string.Empty)
                .Replace('.', '/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        );

    public static string? ContainerPath(string namePath)
    {
        var segments = NormalizeNamePath(namePath)
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length <= 1)
        {
            return null;
        }

        return string.Join('/', segments[..^1]);
    }

    public static string? ContainerName(string namePath)
    {
        var containerPath = ContainerPath(namePath);
        if (string.IsNullOrEmpty(containerPath))
        {
            return null;
        }

        var segments = containerPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length == 0 ? null : segments[^1];
    }
}
