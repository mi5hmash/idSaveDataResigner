using System.Security.Cryptography;
using System.Text;

namespace idSaveDataResigner.Helpers;

public static class IdDeencryption
{
    public const byte NonceLength = 12;
    public const byte TagLength = 16;
    public const byte NonceAndTagTotalLength = NonceLength + TagLength;

    /// <summary>
    /// Decrypts the contents of a file using AES-GCM encryption.
    /// </summary>
    /// <param name="inputSpan">A read-only span of bytes containing the encrypted file data. The span must include the nonce, encrypted data, and authentication tag.</param>
    /// <param name="outputSpan">A writable span of bytes where the decrypted data will be stored.</param>
    /// <param name="fileName">The name of the file being decrypted. This value is used as part of the key derivation process.</param>
    /// <param name="gameCode">A string representing the game code associated with the file. This value is used as part of the key derivation process.</param>
    /// <param name="userId">The user identifier associated with the file. This value is used as part of the key derivation process.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="outputSpan"/> does not have the correct length.</exception>
    public static void DecryptData(ReadOnlySpan<byte> inputSpan, Span<byte> outputSpan, string fileName, string gameCode, string userId)
    {
        // Check if outputSpan is the correct size
        if (outputSpan.Length != inputSpan.Length - NonceAndTagTotalLength)
            throw new ArgumentException("Invalid outputSpan length.");
        // Parse the inputSpan
        var nonce = inputSpan[..NonceLength];
        var encryptedData = inputSpan[NonceLength..^TagLength];
        var tag = inputSpan[^TagLength..];
        // Create key
        using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(userId));
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(gameCode));
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(fileName));
        var key = incrementalHash.GetHashAndReset().AsSpan()[..TagLength];
        // Create authData
        var authData = Encoding.ASCII.GetBytes($"{userId}{gameCode}{fileName}");
        // Decrypt the data
        using var aesGcm = new AesGcm(key, TagLength);
        aesGcm.Decrypt(nonce, encryptedData, tag, outputSpan, authData);
    }

    /// <summary>
    /// Encrypts the input data and writes the encrypted output to the specified span.
    /// </summary>
    /// <param name="inputSpan">The span containing the data to be encrypted.</param>
    /// <param name="outputSpan">The span where the encrypted data will be written. Must be large enough to hold the nonce, encrypted data, and authentication tag.</param>
    /// <param name="fileName">The name of the file associated with the encryption process. Used as part of the encryption key derivation.</param>
    /// <param name="gameCode">A unique code representing the game. Used as part of the encryption key derivation.</param>
    /// <param name="userId">The identifier of the user associated with the input data. Used as part of the encryption key derivation.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="outputSpan"/> does not have the required length to store the encrypted data.</exception>
    public static void EncryptData(ReadOnlySpan<byte> inputSpan, Span<byte> outputSpan, string fileName, string gameCode, string userId)
    {
        // Check if outputSpan is the correct size
        if (outputSpan.Length != inputSpan.Length + NonceAndTagTotalLength)
            throw new ArgumentException("Invalid outputSpan length.");
        // Generate random nonce
        Span<byte> nonce = stackalloc byte[NonceLength];
        RandomNumberGenerator.Fill(nonce);
        // Allocate space for encrypted data
        Span<byte> encryptedData = new byte[inputSpan.Length];
        // Allocate space for tag
        Span<byte> tag = stackalloc byte[TagLength];
        // Create key
        using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(userId));
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(gameCode));
        incrementalHash.AppendData(Encoding.ASCII.GetBytes(fileName));
        var key = incrementalHash.GetHashAndReset().AsSpan()[..TagLength];
        // Create authData
        var authData = Encoding.ASCII.GetBytes($"{userId}{gameCode}{fileName}");
        // Encrypt the data
        using var aesGcm = new AesGcm(key, TagLength);
        aesGcm.Encrypt(nonce, inputSpan, encryptedData, tag, authData);
        // Construct the final output: nonce + encryptedData + tag
        nonce.CopyTo(outputSpan);
        encryptedData.CopyTo(outputSpan[NonceLength..]);
        tag.CopyTo(outputSpan[(NonceLength + encryptedData.Length)..]);
    }
}