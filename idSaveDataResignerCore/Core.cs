using idSaveDataResignerCore.Helpers;
using idSaveDataResignerCore.Infrastructure;
using Mi5hmasH.Logger;

namespace idSaveDataResignerCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts)
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    public async Task DecryptFilesAsync(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
        => await Task.Run(() => DecryptFiles(inputDir, gameCode, userId, cts));

    public void DecryptFiles(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
    {
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        logger.LogInfo($"Decrypting [{filesToProcess.Length}] files...");
        // DECRYPT
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
                catch (Exception e)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to decrypt the [{fileName}] file: {e}", group);
                }
                finally
                {
                    Interlocked.Increment(ref progress);
                    progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
                }
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning(e.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    public async Task EncryptFilesAsync(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
        => await Task.Run(() => EncryptFiles(inputDir, gameCode, userId, cts));

    public void EncryptFiles(string inputDir, string gameCode, string userId, CancellationTokenSource cts)
    {
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        logger.LogInfo($"Encrypting [{filesToProcess.Length}] files...");
        // ENCRYPT
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
                catch (Exception e)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to encrypt the [{fileName}] file: {e}", group);
                }
                finally
                {
                    Interlocked.Increment(ref progress);
                    progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
                }
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning(e.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    public async Task ResignFilesAsync(string inputDir, string gameCode, string userIdInput, string userIdOutput, CancellationTokenSource cts)
        => await Task.Run(() => ResignFiles(inputDir, gameCode, userIdInput, userIdOutput, cts));

    public void ResignFiles(string inputDir, string gameCode, string userIdInput, string userIdOutput, CancellationTokenSource cts)
    {
        var filesToProcess = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return;
        logger.LogInfo($"Resigning [{filesToProcess.Length}] files...");
        // RESIGN
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
                    catch (Exception e)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to decrypt the [{fileName}] file: {e}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                    try
                    {
                        IdDeencryption.EncryptData(encryptedDataSpan, decryptedDataSpan, fileName, gameCode, userIdOutput);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to encrypt the [{fileName}] file: {e}", group);
                        break; // Skip to the next file
                    }
                    // Save the resigned data to the output directory, preserving the folder structure
                    var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                    File.WriteAllBytes(outputFilePath, encryptedDataSpan);
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Resigned the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning(e.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }
}