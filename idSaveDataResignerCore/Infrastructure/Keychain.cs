namespace idSaveDataResignerCore.Infrastructure;

/// <summary>
/// Provides constant values used as cryptographic keys or identifiers within the application.
/// </summary>
/// <remarks>
/// If a program can decrypt something, then the user can as well because the program runs on their machine.
/// </remarks>
public static class Keychain
{
    public const string GpMagic = "czu0hj9U6bS/OUzEXi5NvFqJS7eZSHiFvWudRWBicKU=";
    public const string SettingsMagic = "ogd779BJnqGRTeXiCnuJhWWnmeBjjngN6eJJRrqBJqE=";
}