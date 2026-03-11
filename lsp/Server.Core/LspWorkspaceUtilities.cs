using System.Collections.Generic;
using System.IO;

namespace UnityCli.Lsp.Core;

public static class LspWorkspaceUtilities
{
    public static IEnumerable<string> EnumerateUnityCsFiles(string rootDir)
    {
        IEnumerable<string> Enumerate(string dir)
        {
            if (!Directory.Exists(dir))
            {
                yield break;
            }

            foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                var normalized = file.Replace('\\', '/');
                if (normalized.Contains("/obj/") || normalized.Contains("/bin/"))
                {
                    continue;
                }

                yield return file;
            }
        }

        foreach (var file in Enumerate(Path.Combine(rootDir, "Assets")))
        {
            yield return file;
        }

        foreach (var file in Enumerate(Path.Combine(rootDir, "Packages")))
        {
            yield return file;
        }

        foreach (var file in Enumerate(Path.Combine(rootDir, "Library", "PackageCache")))
        {
            yield return file;
        }
    }
}
