using System.Security.Cryptography;
using System.Text;

namespace TaskMind.InfrastructorLayer.Security;

public static class CitizenIdEncryption
{
    private static byte[]? _masterKey;

    public static void Configure(string? base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
            throw new InvalidOperationException("Security:CitizenIdEncryptionKey must be configured.");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64Key);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("Security:CitizenIdEncryptionKey must be a Base64-encoded 32-byte key.", exception);
        }

        if (key.Length != 32)
            throw new InvalidOperationException("Security:CitizenIdEncryptionKey must decode to exactly 32 bytes.");
        if (_masterKey is not null && !CryptographicOperations.FixedTimeEquals(_masterKey, key))
            throw new InvalidOperationException("CitizenId encryption key cannot be changed while the process is running.");

        _masterKey = key;
    }

    public static string Encrypt(string plaintext)
    {
        var masterKey = GetMasterKey();
        using var aes = Aes.Create();
        aes.Key = DeriveKey(masterKey, "TaskMind.CitizenId.Encryption.v1");
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        var payload = new byte[1 + aes.IV.Length + ciphertext.Length];
        payload[0] = 1;
        Buffer.BlockCopy(aes.IV, 0, payload, 1, aes.IV.Length);
        Buffer.BlockCopy(ciphertext, 0, payload, 1 + aes.IV.Length, ciphertext.Length);

        using var hmac = new HMACSHA256(DeriveKey(masterKey, "TaskMind.CitizenId.Authentication.v1"));
        var tag = hmac.ComputeHash(payload);
        return Convert.ToBase64String([.. payload, .. tag]);
    }

    public static string Decrypt(string protectedValue)
    {
        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(protectedValue);
        }
        catch (FormatException) when (protectedValue.Length == 12 && protectedValue.All(char.IsDigit))
        {
            return protectedValue;
        }

        const int versionAndIvLength = 1 + 16;
        const int tagLength = 32;
        if (payload.Length < versionAndIvLength + 16 + tagLength || payload[0] != 1)
        {
            if (protectedValue.Length == 12 && protectedValue.All(char.IsDigit))
                return protectedValue;
            throw new CryptographicException("CitizenId ciphertext has an unsupported format.");
        }

        var authenticatedLength = payload.Length - tagLength;
        using var hmac = new HMACSHA256(DeriveKey(GetMasterKey(), "TaskMind.CitizenId.Authentication.v1"));
        var expectedTag = hmac.ComputeHash(payload.AsSpan(0, authenticatedLength).ToArray());
        if (!CryptographicOperations.FixedTimeEquals(
                expectedTag,
                payload.AsSpan(authenticatedLength, tagLength)))
            throw new CryptographicException("CitizenId ciphertext authentication failed.");

        var iv = payload.AsSpan(1, 16).ToArray();
        var ciphertextLength = authenticatedLength - versionAndIvLength;
        var ciphertext = payload.AsSpan(versionAndIvLength, ciphertextLength).ToArray();
        using var aes = Aes.Create();
        aes.Key = DeriveKey(GetMasterKey(), "TaskMind.CitizenId.Encryption.v1");
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        var plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] GetMasterKey() =>
        _masterKey ?? throw new InvalidOperationException("CitizenId encryption key has not been configured.");

    private static byte[] DeriveKey(byte[] masterKey, string purpose)
    {
        using var hmac = new HMACSHA256(masterKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(purpose));
    }
}
