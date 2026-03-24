namespace FileRedirector.Wildcards;

/// <summary>
/// Resolves @-wildcards in paths and filename templates.
///
/// Supported tokens (case-insensitive):
///   @YYYY   – 4-digit year          e.g. 2025
///   @YY     – 2-digit year          e.g. 25
///   @MM     – zero-padded month     e.g. 04
///   @M      – month (no pad)        e.g. 4
///   @MNAME  – full month name       e.g. April
///   @MABB   – 3-letter month abbr   e.g. Apr
///   @DD     – zero-padded day       e.g. 07
///   @D      – day (no pad)          e.g. 7
///   @HH     – zero-padded hour (24) e.g. 09
///   @H      – hour (no pad)
///   @MIN    – zero-padded minute    e.g. 05
///   @SS     – zero-padded second
///   @DOW    – numeric day of week   0=Sun … 6=Sat
///   @DOWS   – full day name         e.g. Wednesday
///   @DOWA   – 3-letter abbr         e.g. Wed
///   @WOY    – ISO week of year      e.g. 14
///   @QTR    – quarter               e.g. 2
///   @TICK   – DateTime.UtcNow.Ticks (unique stamp)
///   @GUID   – a new Guid (short, 8 hex chars)
///   @FILE   – original file name (without extension)
///   @EXT    – original file extension (without dot)
///   @ORIGNAME – full original file name with extension
/// </summary>
public static class WildcardEngine
{
    private static readonly (string Token, Func<DateTime, string?, string> Resolver)[] Tokens =
    [
        ("@ORIGNAME", (dt, fn) => fn ?? string.Empty),
        ("@FILE",     (dt, fn) => fn is null ? string.Empty : Path.GetFileNameWithoutExtension(fn)),
        ("@EXT",      (dt, fn) => fn is null ? string.Empty : (Path.GetExtension(fn).TrimStart('.'))),
        ("@YYYY",     (dt, _)  => dt.Year.ToString("D4")),
        ("@YY",       (dt, _)  => dt.ToString("yy")),
        ("@MNAME",    (dt, _)  => dt.ToString("MMMM")),
        ("@MABB",     (dt, _)  => dt.ToString("MMM")),
        ("@MM",       (dt, _)  => dt.ToString("MM")),
        ("@M",        (dt, _)  => dt.Month.ToString()),
        ("@DD",       (dt, _)  => dt.ToString("dd")),
        ("@D",        (dt, _)  => dt.Day.ToString()),
        ("@HH",       (dt, _)  => dt.ToString("HH")),
        ("@H",        (dt, _)  => dt.Hour.ToString()),
        ("@MIN",      (dt, _)  => dt.ToString("mm")),
        ("@SS",       (dt, _)  => dt.ToString("ss")),
        ("@DOWS",     (dt, _)  => dt.DayOfWeek.ToString()),
        ("@DOWA",     (dt, _)  => dt.ToString("ddd")),
        ("@DOW",      (dt, _)  => ((int)dt.DayOfWeek).ToString()),
        ("@WOY",      (dt, _)  => System.Globalization.ISOWeek.GetWeekOfYear(dt).ToString("D2")),
        ("@QTR",      (dt, _)  => ((dt.Month - 1) / 3 + 1).ToString()),
        ("@TICK",     (dt, _)  => DateTime.UtcNow.Ticks.ToString()),
        ("@GUID",     (dt, _)  => Guid.NewGuid().ToString("N")[..8]),
    ];

    /// <summary>Resolves all @-tokens against the given reference time and optional original filename.</summary>
    public static string Resolve(string template, DateTime? referenceTime = null, string? originalFileName = null)
    {
        if (string.IsNullOrEmpty(template)) return template;

        var dt = referenceTime ?? DateTime.Now;
        var result = template;

        // Longest-first order prevents partial-token collisions (e.g. @MM before @M)
        foreach (var (token, resolver) in Tokens)
        {
            if (result.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                // Preserve original casing of the rest of the string
                result = ReplaceIgnoreCase(result, token, resolver(dt, originalFileName));
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a file-pattern that may contain @-wildcards AND glob wildcards
    /// into a Regex that can match file names.
    /// </summary>
    public static System.Text.RegularExpressions.Regex BuildPatternRegex(string pattern, DateTime? referenceTime = null)
    {
        // First resolve date/time tokens so e.g. "Report_@MM@DD*.csv" becomes "Report_0407*.csv"
        var resolved = Resolve(pattern, referenceTime);

        // Then convert glob syntax (* and ?) to regex
        var regexStr = "^" + System.Text.RegularExpressions.Regex.Escape(resolved)
                                  .Replace(@"\*", ".*")
                                  .Replace(@"\?", ".") + "$";

        return new System.Text.RegularExpressions.Regex(regexStr,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.Compiled);
    }

    private static string ReplaceIgnoreCase(string input, string oldValue, string newValue)
    {
        int index = input.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return input;
        return input[..index] + newValue + ReplaceIgnoreCase(input[(index + oldValue.Length)..], oldValue, newValue);
    }

    /// <summary>Returns a preview string showing what all tokens resolve to right now.</summary>
    public static IEnumerable<(string Token, string Example)> GetTokenPreviews()
    {
        var dt = DateTime.Now;
        var sampleFile = "MyReport.pdf";
        foreach (var (token, resolver) in Tokens)
            yield return (token, resolver(dt, sampleFile));
    }
}
