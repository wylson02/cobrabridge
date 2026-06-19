namespace CobraBridge.AccountsService.Tests;

/// <summary>
/// Builds small, self-contained legacy ACCOUNTS.DAT-shaped fixture files for
/// tests, so they don't depend on the real legacy-core/data/ACCOUNTS.DAT
/// (which can change independently of these tests).
/// </summary>
internal static class LegacyFixture
{
    private const int RecordLength = 80;

    public static string BuildRecord(
        string id, string name, string type, string balanceCents, string status)
    {
        var record = id.PadRight(10)
            + name.PadRight(30)
            + type.PadRight(2)
            + balanceCents.PadLeft(11, '0')
            + status;
        return record.PadRight(RecordLength);
    }

    /// <summary>Writes a tiny 3-account legacy file to a temp path and returns it.</summary>
    public static string WriteSampleFile()
    {
        var lines = new[]
        {
            BuildRecord("ACCT000001", "MAISON DUPONT SARL", "CH", "00125000000", "A"),
            BuildRecord("ACCT000002", "ANNA MUELLER", "SV", "00008450050", "A"),
            BuildRecord("ACCT000003", "FROZEN INDUSTRIES", "CH", "00001234567", "F"),
        };

        var path = Path.Combine(Path.GetTempPath(), $"cobrabridge-test-accounts-{Guid.NewGuid()}.dat");
        File.WriteAllLines(path, lines);
        return path;
    }
}
