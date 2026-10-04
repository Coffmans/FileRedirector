using FileRedirector.Wildcards;

namespace FileRedirector.Tests;

public class WildcardEngineTests
{
    // Tuesday 7 April 2026, 09:05:42
    private static readonly DateTime Ref = new(2026, 4, 7, 9, 5, 42);

    [Theory]
    [InlineData("@YYYY", "2026")]
    [InlineData("@YY", "26")]
    [InlineData("@MM", "04")]
    [InlineData("@M", "4")]
    [InlineData("@DD", "07")]
    [InlineData("@D", "7")]
    [InlineData("@HH", "09")]
    [InlineData("@H", "9")]
    [InlineData("@MIN", "05")]
    [InlineData("@SS", "42")]
    [InlineData("@DOW", "2")]
    [InlineData("@DOWS", "Tuesday")]
    [InlineData("@QTR", "2")]
    [InlineData("@WOY", "15")]
    public void Resolves_individual_tokens(string template, string expected)
        => Assert.Equal(expected, WildcardEngine.Resolve(template, Ref));

    [Fact]
    public void Longer_tokens_win_over_their_prefixes()
    {
        // @MIN must not be read as @M + "IN", nor @DOWS as @D + "OWS"
        Assert.Equal("09:05", WildcardEngine.Resolve("@HH:@MIN", Ref));
        Assert.Equal("Tuesday/2/7", WildcardEngine.Resolve("@DOWS/@DOW/@D", Ref));
        Assert.Equal($"{Ref:MMMM}-{Ref:MMM}-04-4", WildcardEngine.Resolve("@MNAME-@MABB-@MM-@M", Ref));
    }

    [Fact]
    public void Tokens_are_case_insensitive()
        => Assert.Equal("2026-04", WildcardEngine.Resolve("@yyyy-@mm", Ref));

    [Fact]
    public void Filename_tokens_use_the_original_name()
        => Assert.Equal("Report_20260407_0905.pdf",
                        WildcardEngine.Resolve("@FILE_@YYYY@MM@DD_@HH@MIN.@EXT", Ref, "Report.pdf"));

    [Fact]
    public void Substituted_values_are_not_rescanned_for_tokens()
        => Assert.Equal("Memo@DD.txt", WildcardEngine.Resolve("@ORIGNAME", Ref, "Memo@DD.txt"));

    [Fact]
    public void Text_without_tokens_is_unchanged()
        => Assert.Equal(@"\\server\share\in", WildcardEngine.Resolve(@"\\server\share\in", Ref));

    [Theory]
    [InlineData("*.csv", "data.csv", true)]
    [InlineData("*.csv", "data.CSV", true)]
    [InlineData("*.csv", "data.csv.done", false)]
    [InlineData("Report_@MM@DD*.csv", "Report_0407_final.csv", true)]
    [InlineData("Report_@MM@DD*.csv", "Report_0408_final.csv", false)]
    [InlineData("file?.txt", "file1.txt", true)]
    [InlineData("file?.txt", "file12.txt", false)]
    [InlineData("a+b(1).txt", "a+b(1).txt", true)]   // regex metacharacters are literal
    public void Pattern_matching(string pattern, string fileName, bool expected)
        => Assert.Equal(expected, WildcardEngine.BuildPatternRegex(pattern, Ref).IsMatch(fileName));
}
