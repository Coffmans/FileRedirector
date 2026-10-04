using FileRedirector.Services;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace FileRedirector.Data;

/// <summary>
/// Encrypts stored credentials with Windows DPAPI (current-user scope): only the Windows account
/// that saved a password can decrypt it. Values are stored as "dpapi:&lt;base64&gt;"; anything
/// without that prefix is treated as legacy plain text.
/// </summary>
internal static class SecretProtector
{
    internal const string Prefix = "dpapi:";
    private static readonly byte[] Entropy = "FileRedirector.credentials.v1"u8.ToArray();

    // Remember values we've already failed to decrypt so the log isn't flooded on every poll
    private static readonly ConcurrentDictionary<string, byte> _reportedFailures = new();

    public static string? Protect(string? plain)
    {
        if (string.IsNullOrEmpty(plain)) return plain;
        var cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), Entropy, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(cipher);
    }

    /// <summary>
    /// Returns the plain-text password, or null if it can't be decrypted (e.g. it was saved by a
    /// different Windows user) — the connection then fails with an authentication error.
    /// </summary>
    public static string? Unprotect(string? stored)
    {
        if (string.IsNullOrEmpty(stored) || !stored.StartsWith(Prefix, StringComparison.Ordinal))
            return stored; // empty or legacy plain text

        try
        {
            var cipher = Convert.FromBase64String(stored[Prefix.Length..]);
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(cipher, Entropy, DataProtectionScope.CurrentUser));
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            if (_reportedFailures.TryAdd(stored, 0))
                AppLog.Error("A saved password could not be decrypted (it may have been saved by a different " +
                             "Windows user). Re-enter it in the job editor.", ex);
            return null;
        }
    }
}
