﻿using idSaveDataResigner.Helpers;
using idSaveDataResigner.Infrastructure;
using idSaveDataResigner.Logger;
using idSaveDataResigner.Logger.Models;
using idSaveDataResigner.Logger.Providers;

#region SETUP

// CONSTANTS
const string breakLine = "---";

// Initialize APP_INFO
var appInfo = new MyAppInfo("idSaveDataResigner");

// Create DIRECTORIES
var directories = new Directories();
directories.CreateAll();

// Initialize LOGGER
var logger = new SimpleLogger
{
    LoggedAppName = appInfo.Name,
    LoggedAppVersion = new Version(MyAppInfo.Version)
};
// Configure ConsoleLogProvider
var consoleLogProvider = new ConsoleLogProvider();
logger.AddProvider(consoleLogProvider);
// Configure FileLogProvider
var fileLogProvider = new FileLogProvider(MyAppInfo.RootPath, 2);
fileLogProvider.CreateLogFile();
logger.AddProvider(fileLogProvider);
// Add event handler for unhandled exceptions
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    if (e.ExceptionObject is not Exception exception) return;
    var logEntry = new LogEntry(SimpleLogger.LogSeverity.Critical, $"Unhandled Exception: {exception}");
    fileLogProvider.Log(logEntry);
    fileLogProvider.Flush();
};
// Flush log providers on process exit
AppDomain.CurrentDomain.ProcessExit += (_, _)
    => logger.Flush();

// Print HEADER
ConsoleHelper.PrintHeader(appInfo, breakLine);

// Say HELLO
ConsoleHelper.SayHello(breakLine);

// Get ARGUMENTS from command line
#if DEBUG
// For debugging purposes, you can manually set the arguments...
if (args.Length < 1)
    args = "-m TEST" // ...here
        .Split(" ");
#endif
var arguments = ConsoleHelper.ReadArguments(args);
#if DEBUG
// Write the arguments to the console for debugging purposes
ConsoleHelper.WriteArguments(arguments);
Console.WriteLine(breakLine);
#endif

#endregion


#region MAIN

// Optional argument: isVerbose
var isVerbose = arguments.ContainsKey("-v");

// Get list of FILES to PROCESS
arguments.TryGetValue("-p", out var inputRootPath);
string[] filesToProcess;
if (Directory.Exists(inputRootPath)) filesToProcess = Directory.GetFiles(inputRootPath, "*.*", SearchOption.AllDirectories);
else
    throw new DirectoryNotFoundException($"The provided path '{inputRootPath}' is not a valid directory or does not exist.");
// Get GAME CODE
arguments.TryGetValue("-g", out var gameCode);
if (string.IsNullOrEmpty(gameCode))
    throw new ArgumentException("Game Code is missing.", nameof(gameCode));
// Get MODE
arguments.TryGetValue("-m", out var mode);
switch (mode)
{
    case "decrypt" or "d":
        // USAGE: -m d -p "FILE_PATH" -g "GAME_CODE" -u 76561197960265729
        DecryptAll();
        break;
    case "encrypt" or "e":
        // USAGE: -m e -p "FILE_PATH" -g "GAME_CODE" -u 76561197960265730
        EncryptAll();
        break;
    case "resign" or "r":
        // USAGE: -m r -p "FILE_PATH" -g "GAME_CODE" -uI 76561197960265729 -uO 76561197960265730
        ResignAll();
        break;
    default:
        throw new ArgumentException($"Unknown mode '{mode}'.", nameof(mode));
}

// EXIT the application
Console.WriteLine(breakLine); // print a break line
ConsoleHelper.SayGoodbye(breakLine);
#if DEBUG
ConsoleHelper.PressAnyKeyToExit();
#else
if (isVerbose) ConsoleHelper.PressAnyKeyToExit();
#endif

return;

#endregion


#region MODES

void DecryptAll()
{
    arguments.TryGetValue("-u", out var userIdInput);
    if (string.IsNullOrEmpty(userIdInput))
        throw new ArgumentException("Input User ID is missing.", nameof(userIdInput));
    if (filesToProcess.Length == 0) return;
    logger.LogInfo($"Decrypting [{filesToProcess.Length}] files...");
    // DECRYPT
    // Create a new folder in OUTPUT directory
    var outputDir = GetNewOutputDirectory(directories.Output, "decrypted");
    // Crate the folder structure in the newly created output directory
    CreateOutputFolderStructure(inputRootPath, outputDir, filesToProcess);
    // Setup parallel options
    CancellationTokenSource cts = new();
    var po = GetParallelOptions(cts);
    // Process files in parallel
    Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
    {
        var fileName = Path.GetFileName(filesToProcess[ctr]);
        var group = $"Task {ctr}";
        ReadOnlySpan<byte> inputDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
        Span<byte> outputDataSpan = new byte[inputDataSpan.Length - IdDeencryption.NonceAndTagTotalLength];
        logger.LogInfo($"Decrypting [{fileName}] file...", group);
        try
        {
            IdDeencryption.DecryptFile(inputDataSpan, outputDataSpan, fileName, gameCode, userIdInput);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to decrypt the [{fileName}] file: {e}", group);
            return; // Skip to the next file
        }
        File.WriteAllBytes(filesToProcess[ctr].Replace(inputRootPath, outputDir), outputDataSpan);
        logger.LogInfo($"Decrypted [{fileName}] file.", group);
    });
}

void EncryptAll()
{
    arguments.TryGetValue("-u", out var userIdOutput);
    if (string.IsNullOrEmpty(userIdOutput))
        throw new ArgumentException("Output User ID is missing.", nameof(userIdOutput));
    if (filesToProcess.Length == 0) return;
    logger.LogInfo($"Encrypting [{filesToProcess.Length}] files...");
    // ENCRYPT
    // Create a new folder in OUTPUT directory
    var outputDir = GetNewOutputDirectory(directories.Output, "encrypted");
    // Crate the folder structure in the newly created output directory
    CreateOutputFolderStructure(inputRootPath, outputDir, filesToProcess);
    // Setup parallel options
    CancellationTokenSource cts = new();
    var po = GetParallelOptions(cts);
    // Process files in parallel
    Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
    {
        var fileName = Path.GetFileName(filesToProcess[ctr]);
        var group = $"Task {ctr}";
        ReadOnlySpan<byte> inputDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
        Span<byte> outputDataSpan = new byte[inputDataSpan.Length + IdDeencryption.NonceAndTagTotalLength];
        logger.LogInfo($"Encrypting [{fileName}] file...", group);
        try
        {
            IdDeencryption.EncryptFile(inputDataSpan, outputDataSpan, fileName, gameCode, userIdOutput);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to encrypt the [{fileName}] file: {e}", group);
            return; // Skip to the next file
        }
        File.WriteAllBytes(filesToProcess[ctr].Replace(inputRootPath, outputDir), outputDataSpan);
        logger.LogInfo($"Encrypted [{fileName}] file.", group);
    });
}

void ResignAll()
{
    arguments.TryGetValue("-uI", out var userIdInput);
    if (string.IsNullOrEmpty(userIdInput))
        throw new ArgumentException("Input User ID is missing.", nameof(userIdInput));
    arguments.TryGetValue("-uO", out var userIdOutput);
    if (string.IsNullOrEmpty(userIdOutput))
        throw new ArgumentException("Output User ID is missing.", nameof(userIdOutput));
    if (filesToProcess.Length == 0) return;
    logger.LogInfo($"Resigning [{filesToProcess.Length}] files...");
    // RESIGN
    // Create a new folder in OUTPUT directory
    var outputDir = GetNewOutputDirectory(directories.Output, "resigned");
    // Crate the folder structure in the newly created output directory
    CreateOutputFolderStructure(inputRootPath, outputDir, filesToProcess);
    // Setup parallel options
    CancellationTokenSource cts = new();
    var po = GetParallelOptions(cts);
    // Process files in parallel
    Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
    {
        var fileName = Path.GetFileName(filesToProcess[ctr]);
        var group = $"Task {ctr}";
        Span<byte> encryptedDataSpan = File.ReadAllBytes(filesToProcess[ctr]);
        Span<byte> decryptedDataSpan = new byte[encryptedDataSpan.Length - IdDeencryption.NonceAndTagTotalLength];
        logger.LogInfo($"Decrypting [{fileName}] file...", group);
        try
        {
            IdDeencryption.DecryptFile(encryptedDataSpan, decryptedDataSpan, fileName, gameCode, userIdInput);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to decrypt the [{fileName}] file: {e}", group);
            return; // Skip to the next file
        }
        logger.LogInfo($"Encrypting [{fileName}] file...", group);
        try
        {
            IdDeencryption.EncryptFile(decryptedDataSpan, encryptedDataSpan, fileName, gameCode, userIdOutput);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to encrypt the [{fileName}] file: {e}", group);
            return; // Skip to the next file
        }
        File.WriteAllBytes(filesToProcess[ctr].Replace(inputRootPath, outputDir), encryptedDataSpan);
        logger.LogInfo($"Resigned [{fileName}] file.", group);
    });
}

#endregion


#region LOCAL_HELPERS

static string GetNewOutputDirectory(string rootPath, string action) => Path.Combine(rootPath, $"{DateTime.Now:yyyy-MM-dd_HHmmssfff}_{action}");

static void CreateOutputFolderStructure(string inputRootPath, string outputDirectory, string[] filesToProcess)
{
    var uniqueParentDirectories = filesToProcess
        .Select(Path.GetDirectoryName)
        .Where(dir => dir != null)
        .Distinct()
        .Select(dir => dir?.Replace(inputRootPath, outputDirectory))
        .ToArray();
    foreach (var dir in uniqueParentDirectories)
    {
        if (dir == null) continue;
        Directory.CreateDirectory(dir);
    }
}

static ParallelOptions GetParallelOptions(CancellationTokenSource cts) =>
    new()
    {
        CancellationToken = cts.Token,
        MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
    };

#endregion