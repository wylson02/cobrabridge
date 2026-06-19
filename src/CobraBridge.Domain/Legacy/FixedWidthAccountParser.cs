namespace CobraBridge.Domain.Legacy;

/// <summary>
/// The load-bearing piece of the anti-corruption layer.
///
/// Parses the legacy 80-byte fixed-width ACCOUNT record (see
/// legacy-core/cobol/copybooks/ACCOUNT.cpy) into a clean domain model.
/// The positional offsets here are a contract with the COBOL copybook:
/// if one moves, both move.
///
/// Shared by the Bridge (serves it live) and the AccountsService's legacy
/// seeder (migrates it into PostgreSQL once) — there is exactly one parser
/// for this format.
///
///   Pos   Len  Field         Picture
///   1     10   ACCT-ID       X(10)
///   11    30   ACCT-NAME     X(30)
///   41     2   ACCT-TYPE     X(02)     CH | SV
///   43    11   ACCT-BALANCE  9(09)V99  implied 2-decimal, in cents
///   54     1   ACCT-STATUS   X(01)     A | C | F
/// </summary>
public static class FixedWidthAccountParser
{
    private const int RecordLength = 80;

    // (zero-based start, length)
    private static readonly (int Start, int Len) IdField      = (0, 10);
    private static readonly (int Start, int Len) NameField    = (10, 30);
    private static readonly (int Start, int Len) TypeField    = (40, 2);
    private static readonly (int Start, int Len) BalanceField = (42, 11);
    private static readonly (int Start, int Len) StatusField  = (53, 1);

    public static IEnumerable<Account> ParseFile(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            yield return ParseRecord(line);
        }
    }

    public static Account ParseRecord(string record)
    {
        if (record.Length < RecordLength)
            throw new FormatException(
                $"Legacy record too short: expected {RecordLength} bytes, got {record.Length}.");

        var balanceDigits = Slice(record, BalanceField).Trim();
        if (!long.TryParse(balanceDigits, out var balanceCents))
            throw new FormatException($"Unparseable balance field: '{balanceDigits}'.");

        return new Account
        {
            Id           = Slice(record, IdField).TrimEnd(),
            Name         = Slice(record, NameField).TrimEnd(),
            Type         = MapType(Slice(record, TypeField)),
            BalanceCents = balanceCents,
            Status       = MapStatus(Slice(record, StatusField))
        };
    }

    private static string Slice(string s, (int Start, int Len) f) =>
        s.Substring(f.Start, f.Len);

    private static AccountType MapType(string raw) => raw.Trim().ToUpperInvariant() switch
    {
        "CH" => AccountType.Checking,
        "SV" => AccountType.Savings,
        _    => AccountType.Unknown
    };

    private static AccountStatus MapStatus(string raw) => raw.Trim().ToUpperInvariant() switch
    {
        "A" => AccountStatus.Active,
        "C" => AccountStatus.Closed,
        "F" => AccountStatus.Frozen,
        _   => AccountStatus.Unknown
    };
}
