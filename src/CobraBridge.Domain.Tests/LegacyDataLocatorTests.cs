using CobraBridge.Domain.Legacy;

namespace CobraBridge.Domain.Tests;

public class LegacyDataLocatorTests
{
    [Fact]
    public void ResolveAccountsFilePath_ExplicitPathConfigured_ReturnsItAsFullPath()
    {
        var configured = Path.Combine(Path.GetTempPath(), "somewhere", "ACCOUNTS.DAT");

        var resolved = LegacyDataLocator.ResolveAccountsFilePath(configured, baseDirectory: Path.GetTempPath());

        Assert.Equal(Path.GetFullPath(configured), resolved);
    }

    [Fact]
    public void ResolveAccountsFilePath_NoConfiguredPath_WalksUpToFindLegacyCoreData()
    {
        var root = Directory.CreateTempSubdirectory("cobrabridge-locator-test-");
        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "legacy-core", "data"));
            var deepStart = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "Some.Project", "bin", "Debug", "net8.0"));

            var resolved = LegacyDataLocator.ResolveAccountsFilePath(configuredPath: null, baseDirectory: deepStart.FullName);

            Assert.Equal(
                Path.Combine(root.FullName, "legacy-core", "data", "ACCOUNTS.DAT"),
                resolved);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void ResolveAccountsFilePath_NoConfiguredPathAndNoLegacyCoreFound_Throws()
    {
        var isolated = Directory.CreateTempSubdirectory("cobrabridge-locator-test-orphan-");
        try
        {
            Assert.Throws<InvalidOperationException>(() =>
                LegacyDataLocator.ResolveAccountsFilePath(configuredPath: null, baseDirectory: isolated.FullName));
        }
        finally
        {
            isolated.Delete(recursive: true);
        }
    }
}
