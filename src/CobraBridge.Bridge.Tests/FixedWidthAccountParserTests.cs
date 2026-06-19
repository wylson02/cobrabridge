using CobraBridge.Bridge.Domain;
using CobraBridge.Bridge.Legacy;

namespace CobraBridge.Bridge.Tests;

public class FixedWidthAccountParserTests
{
    private const int RecordLength = 80;

    // Builds a syntactically valid 80-byte legacy record, matching the
    // ACCOUNT.cpy layout: ACCT-ID(10) ACCT-NAME(30) ACCT-TYPE(2)
    // ACCT-BALANCE(11, cents) ACCT-STATUS(1), space-padded to 80 bytes.
    private static string BuildRecord(
        string id = "ACCT000001",
        string name = "ANNA MUELLER",
        string type = "SV",
        string balanceCents = "00008450050",
        string status = "A")
    {
        var record = id.PadRight(10)
            + name.PadRight(30)
            + type.PadRight(2)
            + balanceCents.PadLeft(11, '0')
            + status;
        return record.PadRight(RecordLength);
    }

    [Fact]
    public void ParseRecord_ValidRecord_MapsEveryField()
    {
        var record = BuildRecord(
            id: "ACCT000002",
            name: "ANNA MUELLER",
            type: "SV",
            balanceCents: "00008450050",
            status: "A");

        var account = FixedWidthAccountParser.ParseRecord(record);

        Assert.Equal("ACCT000002", account.Id);
        Assert.Equal("ANNA MUELLER", account.Name);
        Assert.Equal(AccountType.Savings, account.Type);
        Assert.Equal(8450050L, account.BalanceCents);
        Assert.Equal(AccountStatus.Active, account.Status);
    }

    [Theory]
    [InlineData("CH", AccountType.Checking)]
    [InlineData("SV", AccountType.Savings)]
    [InlineData("ch", AccountType.Checking)]
    [InlineData("XX", AccountType.Unknown)]
    [InlineData("  ", AccountType.Unknown)]
    public void ParseRecord_MapsTypeCode(string typeCode, AccountType expected)
    {
        var account = FixedWidthAccountParser.ParseRecord(BuildRecord(type: typeCode));

        Assert.Equal(expected, account.Type);
    }

    [Theory]
    [InlineData("A", AccountStatus.Active)]
    [InlineData("C", AccountStatus.Closed)]
    [InlineData("F", AccountStatus.Frozen)]
    [InlineData("a", AccountStatus.Active)]
    [InlineData("Z", AccountStatus.Unknown)]
    [InlineData(" ", AccountStatus.Unknown)]
    public void ParseRecord_MapsStatusCode(string statusCode, AccountStatus expected)
    {
        var account = FixedWidthAccountParser.ParseRecord(BuildRecord(status: statusCode));

        Assert.Equal(expected, account.Status);
    }

    [Theory]
    [InlineData("00008450050", 84500.50)]
    [InlineData("00000000000", 0)]
    [InlineData("00999999999", 9999999.99)]
    [InlineData("00000000001", 0.01)]
    public void ParseRecord_ComputesDecimalBalanceFromCents(string balanceCents, decimal expectedBalance)
    {
        var account = FixedWidthAccountParser.ParseRecord(BuildRecord(balanceCents: balanceCents));

        Assert.Equal(expectedBalance, account.Balance);
    }

    [Fact]
    public void ParseRecord_RecordTooShort_ThrowsFormatException()
    {
        var shortRecord = BuildRecord().Substring(0, RecordLength - 1);

        var ex = Assert.Throws<FormatException>(() => FixedWidthAccountParser.ParseRecord(shortRecord));
        Assert.Contains("too short", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseRecord_NonNumericBalance_ThrowsFormatException()
    {
        var record = BuildRecord(balanceCents: "NOTANUMBER!");

        var ex = Assert.Throws<FormatException>(() => FixedWidthAccountParser.ParseRecord(record));
        Assert.Contains("balance", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseFile_SkipsBlankAndWhitespaceLines()
    {
        var lines = new[]
        {
            BuildRecord(id: "ACCT000001"),
            "",
            "   ",
            BuildRecord(id: "ACCT000002"),
        };

        var accounts = FixedWidthAccountParser.ParseFile(lines).ToList();

        Assert.Equal(2, accounts.Count);
        Assert.Equal("ACCT000001", accounts[0].Id);
        Assert.Equal("ACCT000002", accounts[1].Id);
    }
}
