using FileRedirector.Data;

namespace FileRedirector.Tests;

public class SecretProtectorTests
{
    [Fact]
    public void Round_trips_and_does_not_store_plain_text()
    {
        var stored = SecretProtector.Protect("p@ss w0rd!");
        Assert.StartsWith(SecretProtector.Prefix, stored);
        Assert.DoesNotContain("p@ss", stored);
        Assert.Equal("p@ss w0rd!", SecretProtector.Unprotect(stored));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Empty_values_pass_through(string? value)
    {
        Assert.Equal(value, SecretProtector.Protect(value));
        Assert.Equal(value, SecretProtector.Unprotect(value));
    }

    [Fact]
    public void Legacy_plain_text_is_returned_as_is()
        => Assert.Equal("old-secret", SecretProtector.Unprotect("old-secret"));

    [Fact]
    public void Undecryptable_value_returns_null_instead_of_throwing()
        => Assert.Null(SecretProtector.Unprotect(SecretProtector.Prefix + "bm90LXJlYWxseS1lbmNyeXB0ZWQ="));
}
