namespace CobraBridge.Domain.Legacy;

/// <summary>
/// Resolves the path to the legacy ACCOUNTS.DAT master. Shared by every
/// process that needs to read it directly (the Bridge serving it live, the
/// AccountsService's one-time legacy seeder).
///
/// Resolution order:
///   1. An explicit configured path, if given.
///   2. Repo-relative default: walk up from the caller's base directory
///      looking for a "legacy-core/data" folder, so the default works the
///      same whether running from bin/Debug, bin/Release, or `dotnet run`.
/// </summary>
public static class LegacyDataLocator
{
    public static string ResolveAccountsFilePath(string? configuredPath, string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(configuredPath);

        var repoRoot = FindAncestorContaining(baseDirectory, Path.Combine("legacy-core", "data"));
        if (repoRoot is null)
            throw new InvalidOperationException(
                $"Could not locate the 'legacy-core/data' folder above '{baseDirectory}', " +
                "and no explicit accounts file path was configured.");

        return Path.Combine(repoRoot, "legacy-core", "data", "ACCOUNTS.DAT");
    }

    private static string? FindAncestorContaining(string startDirectory, string relativePathToFind)
    {
        for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, relativePathToFind)))
                return dir.FullName;
        }
        return null;
    }
}
