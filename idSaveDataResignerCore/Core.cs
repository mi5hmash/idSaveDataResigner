using idSaveDataResignerCore.Helpers;
using idSaveDataResignerCore.Infrastructure;
using Mi5hmasH.Logger;

namespace idSaveDataResignerCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    /// <summary>
    /// Creates a new ParallelOptions instance configured with the specified cancellation token and an optimal degree of parallelism for the current environment.
    /// </summary>
    /// <param name="cts">The CancellationTokenSource whose token will be used to support cancellation of parallel operations.</param>
    /// <returns>A ParallelOptions object initialized with the provided cancellation token and a maximum degree of parallelism based on the number of available processors.</returns>
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts)
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    /// <summary>
    /// Asynchronously decrypts all files in the specified input directory using the provided game code and user ID.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to decrypt.</param>
    /// <param name="gameCode">The code identifying the game for which the files are to be decrypted.</param>
    /// <param name="userId">The user identifier used for decryption.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the decryption operation.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    public async Task DecryptFilesAsync(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
        => await Task.Run(() => DecryptFiles(inputDir, gameCode, userId, cts));

    /// <summary>
    /// Decrypts all files within the specified input directory and its subdirectories using the provided game code and user ID, saving the decrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to decrypt. All files in this directory and its subdirectories will be processed.</param>
    /// <param name="gameCode">The game code used for decryption. This value must correspond to the encryption context of the files.</param>
    /// <param name="userId">The user identifier used for decryption and for organizing the output directory.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the decryption operation. If cancellation is requested, the operation will terminate early.</param>
    public void DecryptFiles(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        // DECRYPT
        logger.LogInfo($"Decrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("decrypted").AddUserId(userId);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                var fileName = Path.GetFileName(filesToProcess[ctr]);
                var group = $"Task {ctr}";
                try
                {
                    ReadOnlySpan<byte> inputDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
                    Span<byte> outputDataSpan = new byte[inputDataSpan.Length - IdDeencryption.NonceAndTagTotalLength];
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypting the [{fileName}] file...", group);
                    IdDeencryption.DecryptData(outputDataSpan, inputDataSpan, fileName, gameCode, userId);
                    // Save the decrypted data to the output directory, preserving the folder structure
                    var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                    File.WriteAllBytes(outputFilePath, outputDataSpan);
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypted the [{fileName}] file.", group);
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to decrypt the [{fileName}] file: {ex}", group);
                }
                finally
                {
                    Interlocked.Increment(ref progress);
                    progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
                }
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously encrypts all files in the specified input directory using the provided game code and user ID.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to encrypt. Must not be null or empty.</param>
    /// <param name="gameCode">The game code used to determine encryption parameters. Must not be null or empty.</param>
    /// <param name="userId">The user identifier associated with the encryption operation. Must not be null or empty.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the encryption operation.</param>
    /// <returns>A task that represents the asynchronous encryption operation.</returns>
    public async Task EncryptFilesAsync(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
        => await Task.Run(() => EncryptFiles(inputDir, gameCode, userId, cts));

    /// <summary>
    /// Encrypts all files within the specified input directory and its subdirectories, saving the encrypted files to a new output directory while preserving the original folder structure.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to encrypt. All files in this directory and its subdirectories will be processed.</param>
    /// <param name="gameCode">A string representing the game code used as part of the encryption process for each file.</param>
    /// <param name="userId">The user identifier associated with the encryption operation. Used to organize output and as part of the encryption metadata.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the encryption operation before completion.</param>
    public void EncryptFiles(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        // ENCRYPT
        logger.LogInfo($"Encrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("encrypted").AddUserId(userId);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                var fileName = Path.GetFileName(filesToProcess[ctr]);
                var group = $"Task {ctr}";
                try
                {
                    ReadOnlySpan<byte> inputDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
                    Span<byte> outputDataSpan = new byte[inputDataSpan.Length + IdDeencryption.NonceAndTagTotalLength];
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                    IdDeencryption.EncryptData(outputDataSpan, inputDataSpan, fileName, gameCode, userId);
                    // Save the encrypted data to the output directory, preserving the folder structure
                    var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                    File.WriteAllBytes(outputFilePath, outputDataSpan);
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypted the [{fileName}] file.", group);
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to encrypt the [{fileName}] file: {ex}", group);
                }
                finally
                {
                    Interlocked.Increment(ref progress);
                    progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
                }
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously re-signs all files in the specified input directory using the provided game code and user IDs.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to be re-signed.</param>
    /// <param name="gameCode">The game code used for the re-signing process.</param>
    /// <param name="userIdInput">The user ID associated with the original file signatures.</param>
    /// <param name="userIdOutput">The user ID to use for the new file signatures.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the operation before completion.</param>
    /// <returns>A task that represents the asynchronous re-signing operation.</returns>
    public async Task ResignFilesAsync(string inputDir, string gameCode, string userIdInput, string userIdOutput, CancellationTokenSource cts)
        => await Task.Run(() => ResignFiles(inputDir, gameCode, userIdInput, userIdOutput, cts));

    /// <summary>
    /// Re-signs all files in the specified input directory by decrypting them with the original user ID and re-encrypting them
    /// with a new user ID. The re-signed files are saved to a newly created output directory, preserving the original folder structure.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to be re-signed. All files within this directory and its subdirectories will be processed.</param>
    /// <param name="gameCode">The game code used for cryptographic operations during decryption and encryption.</param>
    /// <param name="userIdInput">The user ID associated with the original encryption of the files. Used to decrypt the input files.</param>
    /// <param name="userIdOutput">The user ID to use when re-encrypting the files. The re-signed files will be encrypted with this user ID.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the re-signing operation. If cancellation is requested, the process will terminate early.</param>
    public void ResignFiles(string inputDir, string gameCode, string userIdInput, string userIdOutput, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        // RE-SIGN
        logger.LogInfo($"Re-signing [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserId(userIdOutput);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";
                    Span<byte> encryptedDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
                    Span<byte> decryptedDataSpan = new byte[encryptedDataSpan.Length - IdDeencryption.NonceAndTagTotalLength];
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypting the [{fileName}] file...", group);
                    try
                    {
                        IdDeencryption.DecryptData(decryptedDataSpan, encryptedDataSpan, fileName, gameCode, userIdInput);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to decrypt the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                    try
                    {
                        IdDeencryption.EncryptData(encryptedDataSpan, decryptedDataSpan, fileName, gameCode, userIdOutput);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to encrypt the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Save the re-signed data to the output directory, preserving the folder structure
                    var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                    File.WriteAllBytes(outputFilePath, encryptedDataSpan);
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Re-signed the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }
}