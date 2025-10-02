using idSaveDataResignerCore;
using idSaveDataResignerCore.Helpers;
using Mi5hmasH.Logger;

namespace QualityControl.xUnit;

public sealed class IdSdrCoreTests : IDisposable
{
    private readonly Core _core;
    private readonly ITestOutputHelper _output;

    public IdSdrCoreTests(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("SETUP");

        // Setup
        var logger = new SimpleLogger();
        var progressReporter = new ProgressReporter(null, null);
        _core = new Core(logger, progressReporter);
    }

    public void Dispose()
    {
        _output.WriteLine("CLEANUP");
    }
    
    [Fact]
    public async Task DecryptFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.DecryptFilesAsync(tempDir, "gameCode", "userId", cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task EncryptFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.EncryptFilesAsync(tempDir, "gameCode", "userId", cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task ResignFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.ResignFilesAsync(tempDir, "gameCode", "userIdInput", "userIdOutput", cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void DecryptFiles_DoesDecrypt()
    {
        // Arrange
        const string fileName = "game.details";
        const string gameCode = "MANCUBUS";
        const string userId = "76561197960265729";
        Span<byte> decryptedDataSpan = new byte[Properties.Resources.encryptedFile.Length - IdDeencryption.NonceAndTagTotalLength];

        // Act
        IdDeencryption.DecryptData(decryptedDataSpan, Properties.Resources.encryptedFile, fileName, gameCode, userId);
        
        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)decryptedDataSpan);
    }

    [Fact]
    public void EncryptFiles_DoesEncrypt()
    {
        // Arrange
        const string fileName = "game.details";
        const string gameCode = "MANCUBUS";
        const string userId = "76561197960265729";
        Span<byte> encryptedDataSpan = new byte[Properties.Resources.encryptedFile.Length];
        Span<byte> decryptedDataSpan = new byte[encryptedDataSpan.Length - IdDeencryption.NonceAndTagTotalLength];

        // Act
        IdDeencryption.EncryptData(encryptedDataSpan, Properties.Resources.decryptedFile, fileName, gameCode, userId);
        IdDeencryption.DecryptData(decryptedDataSpan, encryptedDataSpan, fileName, gameCode, userId);

        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)decryptedDataSpan);
    }
}